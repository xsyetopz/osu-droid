package moe.osudroid.tools;

import java.io.BufferedInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.Reader;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.time.Instant;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Set;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

public final class AssetToolsMain {
    private static final Gson GSON = new GsonBuilder().setPrettyPrinting().create();

    private AssetToolsMain() {
    }

    public static void main(String[] args) {
        if (args.length == 0) {
            printHelp();
            return;
        }

        if ("sync-ui".equals(args[0])) {
            try {
                syncUi();
            } catch (IOException e) {
                throw new RuntimeException("Failed to sync upstream UI metadata.", e);
            }
            return;
        }

        printHelp();
    }

    private static void syncUi() throws IOException {
        File repoRoot = findRepoRoot();
        UpstreamLock lock = loadLock(repoRoot);
        validateLock(lock);

        Map<String, ResolvedSource> resolvedSources = resolveSources(repoRoot, lock.sources);
        String generatedAt = Instant.now().toString();

        ThemeBundleContract contract = buildContract(lock, resolvedSources, generatedAt);
        ThemeManifest manifest = buildManifest(lock, contract, resolvedSources, generatedAt);
        ThemeStagingIndex stagingIndex = buildStagingIndex(lock, contract, generatedAt);

        writeJson(new File(repoRoot, lock.themeBundle.contractPath), contract);
        writeJson(new File(repoRoot, lock.themeBundle.manifestPath), manifest);
        writeJson(new File(repoRoot, lock.themeBundle.stagingIndexPath), stagingIndex);
        writeNotice(new File(repoRoot, lock.themeBundle.noticePath), lock, contract, generatedAt);

        System.out.println("Prepared upstream-derived theme bundle contract with " + contract.entries.size()
                + " entries from " + contract.sources.size() + " sources.");
    }

    private static UpstreamLock loadLock(File repoRoot) throws IOException {
        File lockFile = new File(repoRoot, "upstream-sources.lock.json");
        Reader reader = new FileReader(lockFile);
        try {
            return GSON.fromJson(reader, UpstreamLock.class);
        } finally {
            reader.close();
        }
    }

    private static void validateLock(UpstreamLock lock) {
        if (lock == null) {
            throw new IllegalStateException("upstream-sources.lock.json is empty or invalid.");
        }
        if (lock.schemaVersion < 2) {
            throw new IllegalStateException("upstream-sources.lock.json schemaVersion must be >= 2.");
        }
        if (lock.themeBundle == null) {
            throw new IllegalStateException("upstream-sources.lock.json is missing themeBundle configuration.");
        }
        if (lock.themeBundle.includeRoots == null || lock.themeBundle.includeRoots.isEmpty()) {
            throw new IllegalStateException("themeBundle.includeRoots must define at least one upstream include root.");
        }
        if (lock.sources == null || lock.sources.isEmpty()) {
            throw new IllegalStateException("sources[] must define at least one upstream repository.");
        }
        if (lock.brandingPolicy == null || !lock.brandingPolicy.replaceUpstreamMarks) {
            throw new IllegalStateException("brandingPolicy.replaceUpstreamMarks must stay true for synced bundles.");
        }
    }

    private static Map<String, ResolvedSource> resolveSources(File repoRoot, List<UpstreamSource> sources) throws IOException {
        Map<String, ResolvedSource> resolved = new HashMap<String, ResolvedSource>();
        for (UpstreamSource source : sources) {
            if (source == null || source.id == null || source.id.isEmpty()) {
                throw new IllegalStateException("Each source entry requires a non-empty id.");
            }
            if (source.pinnedRevision == null || source.pinnedRevision.trim().isEmpty()
                    || "UNSYNCED".equalsIgnoreCase(source.pinnedRevision.trim())) {
                throw new IllegalStateException(
                        "Pinned revision missing for " + source.id + " in upstream-sources.lock.json.");
            }

            File sourceDir = resolveSourceDirectory(repoRoot, source);
            if (!sourceDir.isDirectory()) {
                throw new IllegalStateException("Missing upstream source checkout: " + sourceDir.getCanonicalPath());
            }

            String revision = gitHeadRevision(sourceDir);
            if (!revision.startsWith(source.pinnedRevision)) {
                throw new IllegalStateException("Pinned revision mismatch for " + source.id + ": expected "
                        + source.pinnedRevision + ", found " + revision + ".");
            }

            ResolvedSource resolvedSource = new ResolvedSource();
            resolvedSource.source = source;
            resolvedSource.directory = sourceDir;
            resolvedSource.revision = revision;
            resolved.put(source.id, resolvedSource);
        }
        return resolved;
    }

    private static File resolveSourceDirectory(File repoRoot, UpstreamSource source) throws IOException {
        String configuredPath = resolvePath(source);
        File directory = new File(configuredPath);
        if (!directory.isAbsolute()) {
            directory = new File(repoRoot, configuredPath);
        }
        return directory.getCanonicalFile();
    }

    private static String gitHeadRevision(File sourceDir) throws IOException {
        ProcessBuilder builder = new ProcessBuilder("git", "-C", sourceDir.getAbsolutePath(), "rev-parse", "HEAD");
        builder.redirectErrorStream(true);
        Process process = builder.start();
        String output = readProcessOutput(process.getInputStream()).trim();
        int exitCode;
        try {
            exitCode = process.waitFor();
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new IOException("Interrupted while reading revision from " + sourceDir.getCanonicalPath(), e);
        }
        if (exitCode != 0 || output.isEmpty()) {
            throw new IllegalStateException("Unable to resolve git revision for " + sourceDir.getCanonicalPath() + ": " + output);
        }
        return output;
    }

    private static String readProcessOutput(InputStream inputStream) throws IOException {
        ByteArrayOutputStream buffer = new ByteArrayOutputStream();
        byte[] chunk = new byte[4096];
        int read;
        while ((read = inputStream.read(chunk)) != -1) {
            buffer.write(chunk, 0, read);
        }
        return new String(buffer.toByteArray(), StandardCharsets.UTF_8);
    }

    private static ThemeBundleContract buildContract(
            UpstreamLock lock,
            Map<String, ResolvedSource> resolvedSources,
            String generatedAt) throws IOException {
        ThemeBundleContract contract = new ThemeBundleContract();
        contract.schemaVersion = lock.themeBundle.schemaVersion;
        contract.bundleId = lock.themeBundle.id;
        contract.bundleName = lock.themeBundle.name;
        contract.generatedAtUtc = generatedAt;
        contract.brandingPolicy = lock.brandingPolicy;
        contract.sources = new ArrayList<ContractSource>();
        contract.entries = new ArrayList<ContractEntry>();
        contract.includeRoots = new ArrayList<ContractIncludeRoot>();

        Set<String> allowedExtensions = normalizedExtensions(lock.themeBundle.allowedExtensions);
        int maxEntries = lock.themeBundle.maxContractEntries <= 0 ? Integer.MAX_VALUE : lock.themeBundle.maxContractEntries;

        for (ThemeIncludeRoot includeRoot : lock.themeBundle.includeRoots) {
            ResolvedSource source = resolvedSources.get(includeRoot.sourceId);
            if (source == null) {
                throw new IllegalStateException("themeBundle.includeRoots references unknown sourceId: " + includeRoot.sourceId);
            }

            File root = new File(source.directory, includeRoot.sourcePath);
            if (!root.isDirectory()) {
                throw new IllegalStateException("Missing include root " + includeRoot.sourcePath + " in "
                        + source.directory.getCanonicalPath());
            }

            List<ContractEntry> entries = collectEntries(
                    root.toPath(),
                    includeRoot.outputPrefix,
                    source,
                    allowedExtensions,
                    maxEntries - contract.entries.size());
            if (entries.isEmpty()) {
                throw new IllegalStateException("Include root has no matching files: " + root.getCanonicalPath());
            }

            contract.entries.addAll(entries);
            ContractIncludeRoot contractIncludeRoot = new ContractIncludeRoot();
            contractIncludeRoot.sourceId = includeRoot.sourceId;
            contractIncludeRoot.sourcePath = includeRoot.sourcePath;
            contractIncludeRoot.outputPrefix = includeRoot.outputPrefix;
            contractIncludeRoot.entryCount = entries.size();
            contract.includeRoots.add(contractIncludeRoot);
        }

        if (contract.entries.isEmpty()) {
            throw new IllegalStateException("No theme entries were collected from configured include roots.");
        }

        for (ResolvedSource source : resolvedSources.values()) {
            ContractSource contractSource = new ContractSource();
            contractSource.id = source.source.id;
            contractSource.displayName = source.source.displayName;
            contractSource.repoUrl = source.source.repoUrl;
            contractSource.checkoutPath = source.directory.getCanonicalPath();
            contractSource.pinnedRevision = source.source.pinnedRevision;
            contractSource.resolvedRevision = source.revision;
            contractSource.license = source.source.license;
            contractSource.attribution = source.source.attribution;
            contractSource.entryCount = countEntriesForSource(contract.entries, source.source.id);
            contract.sources.add(contractSource);
        }
        Collections.sort(contract.sources, new Comparator<ContractSource>() {
            @Override
            public int compare(ContractSource left, ContractSource right) {
                return left.id.compareTo(right.id);
            }
        });

        return contract;
    }

    private static ThemeManifest buildManifest(
            UpstreamLock lock,
            ThemeBundleContract contract,
            Map<String, ResolvedSource> resolvedSources,
            String generatedAt) {
        ThemeManifest manifest = new ThemeManifest();
        manifest.name = lock.themeBundle.name;
        manifest.version = lock.themeBundle.schemaVersion;
        manifest.brandName = lock.brandingPolicy.replacementBrandName;
        manifest.brandSubtitle = "Shared Android + iOS rewrite shell";
        manifest.defaultRoute = lock.themeBundle.defaultRoute;
        manifest.palette = defaultPalette();
        manifest.assets = new ArrayList<ManifestAsset>();
        manifest.notices = new ArrayList<ManifestNotice>();
        manifest.provenance = new ManifestProvenance();
        manifest.brandingPolicy = lock.brandingPolicy;

        ManifestAsset chrome = new ManifestAsset();
        chrome.key = "CHROME_PIXEL";
        chrome.path = "bootstrap/pixel.png";
        chrome.origin = "LOCAL_OVERRIDE";
        manifest.assets.add(chrome);

        ManifestAsset contractAsset = new ManifestAsset();
        contractAsset.key = "UPSTREAM_THEME_CONTRACT";
        contractAsset.path = toAssetRelativePath(lock.themeBundle.contractPath);
        contractAsset.origin = "UPSTREAM_SYNC_CONTRACT";
        manifest.assets.add(contractAsset);

        for (ContractSource source : contract.sources) {
            ManifestNotice notice = new ManifestNotice();
            notice.sourceName = source.displayName == null ? source.id : source.displayName;
            notice.sourceUrl = source.repoUrl;
            notice.license = source.license;
            notice.notes = "Pinned " + source.pinnedRevision + " (resolved " + source.resolvedRevision
                    + "), entries " + source.entryCount + ".";
            manifest.notices.add(notice);
        }

        ManifestNotice policyNotice = new ManifestNotice();
        policyNotice.sourceName = "Branding replacement policy";
        policyNotice.sourceUrl = "local://upstream-sources.lock.json";
        policyNotice.license = "Build policy";
        policyNotice.notes = lock.brandingPolicy.notes;
        manifest.notices.add(policyNotice);

        manifest.provenance.generatedAtUtc = generatedAt;
        manifest.provenance.bundleId = contract.bundleId;
        manifest.provenance.contractPath = toAssetRelativePath(lock.themeBundle.contractPath);
        manifest.provenance.noticePath = toAssetRelativePath(lock.themeBundle.noticePath);
        manifest.provenance.stagingIndexPath = lock.themeBundle.stagingIndexPath;
        manifest.provenance.syncCommand = "./gradlew tools:run --args=\"sync-ui\"";
        manifest.provenance.sourcePins = new ArrayList<ManifestSourcePin>();
        for (ResolvedSource source : resolvedSources.values()) {
            ManifestSourcePin pin = new ManifestSourcePin();
            pin.id = source.source.id;
            pin.repoUrl = source.source.repoUrl;
            pin.checkoutPath = source.directory.getAbsolutePath();
            pin.pinnedRevision = source.source.pinnedRevision;
            pin.resolvedRevision = source.revision;
            manifest.provenance.sourcePins.add(pin);
        }
        Collections.sort(manifest.provenance.sourcePins, new Comparator<ManifestSourcePin>() {
            @Override
            public int compare(ManifestSourcePin left, ManifestSourcePin right) {
                return left.id.compareTo(right.id);
            }
        });
        return manifest;
    }

    private static ThemeStagingIndex buildStagingIndex(UpstreamLock lock, ThemeBundleContract contract, String generatedAt) {
        ThemeStagingIndex index = new ThemeStagingIndex();
        index.schemaVersion = 1;
        index.generatedAtUtc = generatedAt;
        index.bundleId = contract.bundleId;
        index.contractPath = lock.themeBundle.contractPath;
        index.noticePath = lock.themeBundle.noticePath;
        index.totalEntries = contract.entries.size();
        index.roots = contract.includeRoots;
        return index;
    }

    private static int countEntriesForSource(List<ContractEntry> entries, String sourceId) {
        int count = 0;
        for (ContractEntry entry : entries) {
            if (sourceId.equals(entry.sourceId)) {
                count++;
            }
        }
        return count;
    }

    private static List<ContractEntry> collectEntries(
            Path root,
            String outputPrefix,
            ResolvedSource source,
            Set<String> allowedExtensions,
            int remainingBudget) throws IOException {
        List<Path> files;
        Stream<Path> walk = Files.walk(root);
        try {
            files = walk.filter(new java.util.function.Predicate<Path>() {
                @Override
                public boolean test(Path path) {
                    return Files.isRegularFile(path);
                }
            }).sorted().collect(Collectors.toList());
        } finally {
            walk.close();
        }

        List<ContractEntry> entries = new ArrayList<ContractEntry>();
        String normalizedPrefix = trimPath(outputPrefix);
        for (Path file : files) {
            String extension = extensionOf(file.getFileName().toString());
            if (!allowedExtensions.isEmpty() && !allowedExtensions.contains(extension)) {
                continue;
            }
            if (entries.size() >= remainingBudget) {
                throw new IllegalStateException("themeBundle.maxContractEntries exceeded while scanning " + root.toString());
            }
            ContractEntry entry = new ContractEntry();
            entry.sourceId = source.source.id;
            entry.sourcePath = toUnixPath(root.relativize(file));
            entry.sourceRoot = toUnixPath(root.toAbsolutePath());
            entry.outputPath = normalizedPrefix.isEmpty()
                    ? entry.sourcePath
                    : normalizedPrefix + "/" + entry.sourcePath;
            entry.sha256 = sha256(file);
            entry.sizeBytes = Files.size(file);
            entry.pinnedRevision = source.source.pinnedRevision;
            entry.resolvedRevision = source.revision;
            entry.license = source.source.license;
            entry.brandingAction = "REPLACE_UPSTREAM_MARKS_BEFORE_RELEASE";
            entries.add(entry);
        }
        return entries;
    }

    private static Set<String> normalizedExtensions(List<String> allowedExtensions) {
        if (allowedExtensions == null || allowedExtensions.isEmpty()) {
            return Collections.emptySet();
        }
        Set<String> normalized = new HashSet<String>();
        for (String extension : allowedExtensions) {
            if (extension == null || extension.trim().isEmpty()) {
                continue;
            }
            String lowercase = extension.trim().toLowerCase(Locale.ROOT);
            normalized.add(lowercase.startsWith(".") ? lowercase : "." + lowercase);
        }
        return normalized;
    }

    private static String extensionOf(String fileName) {
        int dot = fileName.lastIndexOf('.');
        if (dot < 0 || dot == fileName.length() - 1) {
            return "";
        }
        return fileName.substring(dot).toLowerCase(Locale.ROOT);
    }

    private static String sha256(Path file) throws IOException {
        MessageDigest digest;
        try {
            digest = MessageDigest.getInstance("SHA-256");
        } catch (NoSuchAlgorithmException e) {
            throw new IllegalStateException("Missing SHA-256 digest implementation.", e);
        }

        InputStream fileInputStream = new FileInputStream(file.toFile());
        BufferedInputStream inputStream = new BufferedInputStream(fileInputStream);
        try {
            byte[] buffer = new byte[8192];
            int read;
            while ((read = inputStream.read(buffer)) >= 0) {
                if (read > 0) {
                    digest.update(buffer, 0, read);
                }
            }
        } finally {
            inputStream.close();
            fileInputStream.close();
        }

        byte[] bytes = digest.digest();
        StringBuilder result = new StringBuilder(bytes.length * 2);
        for (byte value : bytes) {
            result.append(String.format(Locale.ROOT, "%02x", value));
        }
        return result.toString();
    }

    private static ThemePalette defaultPalette() {
        ThemePalette palette = new ThemePalette();
        palette.background = "#101821";
        palette.panel = "#172739";
        palette.panelAlt = "#1E344A";
        palette.accent = "#49BFF8";
        palette.accentSoft = "#2F6E91";
        palette.textPrimary = "#F5FBFF";
        palette.textMuted = "#A4B5C6";
        palette.success = "#5BD690";
        palette.danger = "#FF7596";
        return palette;
    }

    private static String toAssetRelativePath(String path) {
        String normalized = trimPath(path);
        if (normalized.startsWith("assets/")) {
            return normalized.substring("assets/".length());
        }
        return normalized;
    }

    private static String trimPath(String path) {
        if (path == null) {
            return "";
        }
        return path.replace('\\', '/').replaceAll("^/+", "").replaceAll("/+$", "");
    }

    private static String toUnixPath(Path path) {
        return path.toString().replace(File.separatorChar, '/');
    }

    private static File findRepoRoot() throws IOException {
        File current = new File(".").getCanonicalFile();
        while (current != null) {
            File marker = new File(current, "upstream-sources.lock.json");
            if (marker.exists()) {
                return current;
            }
            current = current.getParentFile();
        }
        throw new IllegalStateException("Could not locate upstream-sources.lock.json from the current working directory.");
    }

    private static String resolvePath(UpstreamSource source) {
        String envKey = source.id.toUpperCase().replace('-', '_') + "_PATH";
        String envValue = System.getenv(envKey);
        return envValue == null || envValue.isEmpty() ? source.defaultPath : envValue;
    }

    private static void writeJson(File target, Object value) throws IOException {
        File parent = target.getParentFile();
        if (parent != null && !parent.exists() && !parent.mkdirs()) {
            throw new IOException("Failed to create directory " + parent.getCanonicalPath());
        }
        Writer writer = new FileWriter(target);
        try {
            GSON.toJson(value, writer);
        } finally {
            writer.close();
        }
    }

    private static void writeNotice(
            File noticeFile,
            UpstreamLock lock,
            ThemeBundleContract contract,
            String generatedAt) throws IOException {
        File parent = noticeFile.getParentFile();
        if (parent != null && !parent.exists() && !parent.mkdirs()) {
            throw new IOException("Failed to create directory " + parent.getCanonicalPath());
        }
        Writer writer = new FileWriter(noticeFile);
        try {
            writer.write("Generated by sync-ui at " + generatedAt + "\n");
            writer.write("Theme bundle: " + contract.bundleId + " (" + contract.bundleName + ")\n\n");

            writer.write("Upstream provenance:\n");
            for (ContractSource source : contract.sources) {
                writer.write("- " + source.id + " (" + source.displayName + ")\n");
                writer.write("  repo: " + source.repoUrl + "\n");
                writer.write("  checkout: " + source.checkoutPath + "\n");
                writer.write("  pinned revision: " + source.pinnedRevision + "\n");
                writer.write("  resolved revision: " + source.resolvedRevision + "\n");
                writer.write("  license: " + source.license + "\n");
                writer.write("  attribution: " + source.attribution + "\n");
                writer.write("  contracted entries: " + source.entryCount + "\n\n");
            }

            writer.write("Branding replacement policy:\n");
            writer.write("- replace upstream marks: " + lock.brandingPolicy.replaceUpstreamMarks + "\n");
            writer.write("- forbidden marks: "
                    + (lock.brandingPolicy.forbiddenMarks == null
                            ? "(none)"
                            : String.join(", ", lock.brandingPolicy.forbiddenMarks))
                    + "\n");
            writer.write("- replacement brand name: " + lock.brandingPolicy.replacementBrandName + "\n");
            writer.write("- notes: " + lock.brandingPolicy.notes + "\n\n");

            writer.write("Bundle contract: " + lock.themeBundle.contractPath + "\n");
            writer.write("Staging index: " + lock.themeBundle.stagingIndexPath + "\n");
            writer.write("Total entries: " + contract.entries.size() + "\n");
        } finally {
            writer.close();
        }
    }

    private static void printHelp() {
        System.out.println("Asset tools commands:");
        System.out.println(Arrays.asList("  sync-ui"));
    }

    private static final class UpstreamLock {
        int schemaVersion;
        ThemeBundle themeBundle;
        List<UpstreamSource> sources;
        BrandingPolicy brandingPolicy;
    }

    private static final class ThemeBundle {
        int schemaVersion;
        String id;
        String name;
        String manifestPath;
        String noticePath;
        String contractPath;
        String stagingIndexPath;
        String defaultRoute;
        int maxContractEntries;
        List<String> allowedExtensions;
        List<ThemeIncludeRoot> includeRoots;
    }

    private static final class ThemeIncludeRoot {
        String sourceId;
        String sourcePath;
        String outputPrefix;
    }

    private static final class UpstreamSource {
        String id;
        String displayName;
        String repoUrl;
        String defaultPath;
        String pinnedRevision;
        String license;
        String attribution;
    }

    private static final class BrandingPolicy {
        boolean replaceUpstreamMarks;
        List<String> forbiddenMarks;
        String replacementBrandName;
        String notes;
    }

    private static final class ResolvedSource {
        UpstreamSource source;
        File directory;
        String revision;
    }

    private static final class ThemeBundleContract {
        int schemaVersion;
        String bundleId;
        String bundleName;
        String generatedAtUtc;
        BrandingPolicy brandingPolicy;
        List<ContractSource> sources;
        List<ContractIncludeRoot> includeRoots;
        List<ContractEntry> entries;
    }

    private static final class ContractSource {
        String id;
        String displayName;
        String repoUrl;
        String checkoutPath;
        String pinnedRevision;
        String resolvedRevision;
        String license;
        String attribution;
        int entryCount;
    }

    private static final class ContractIncludeRoot {
        String sourceId;
        String sourcePath;
        String outputPrefix;
        int entryCount;
    }

    private static final class ContractEntry {
        String sourceId;
        String sourceRoot;
        String sourcePath;
        String outputPath;
        String sha256;
        long sizeBytes;
        String pinnedRevision;
        String resolvedRevision;
        String license;
        String brandingAction;
    }

    private static final class ThemeManifest {
        String name;
        int version;
        String brandName;
        String brandSubtitle;
        String defaultRoute;
        ThemePalette palette;
        List<ManifestAsset> assets;
        List<ManifestNotice> notices;
        ManifestProvenance provenance;
        BrandingPolicy brandingPolicy;
    }

    private static final class ThemePalette {
        String background;
        String panel;
        String panelAlt;
        String accent;
        String accentSoft;
        String textPrimary;
        String textMuted;
        String success;
        String danger;
    }

    private static final class ManifestAsset {
        String key;
        String path;
        String origin;
    }

    private static final class ManifestNotice {
        String sourceName;
        String sourceUrl;
        String license;
        String notes;
    }

    private static final class ManifestProvenance {
        String generatedAtUtc;
        String bundleId;
        String contractPath;
        String noticePath;
        String stagingIndexPath;
        String syncCommand;
        List<ManifestSourcePin> sourcePins;
    }

    private static final class ManifestSourcePin {
        String id;
        String repoUrl;
        String checkoutPath;
        String pinnedRevision;
        String resolvedRevision;
    }

    private static final class ThemeStagingIndex {
        int schemaVersion;
        String generatedAtUtc;
        String bundleId;
        String contractPath;
        String noticePath;
        int totalEntries;
        List<ContractIncludeRoot> roots;
    }
}
