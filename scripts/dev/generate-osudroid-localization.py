#!/usr/bin/env python3
from __future__ import annotations

import argparse
import html
import re
import sys
import xml.etree.ElementTree as ET
from collections import OrderedDict
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
OSUDROID_SOURCE = REPO / "third_party" / "osu-droid-legacy"
LANGUAGE_PACK = REPO / "third_party" / "osu-droid-language-pack"
RESX = REPO / "src" / "OsuDroid.Game" / "Localization" / "Strings.resx"
LOCALE_TEMPLATE = REPO / "src" / "OsuDroid.Game" / "Localization" / "Strings.locale.resx.template"

LANGUAGE_PACK_KEY = "language-pack"
OSUDROID_KEY = "osudroid"
LITERAL_KEY = "literal"

APP_ALIASES: "OrderedDict[str, tuple[str, str]]" = OrderedDict([
    ("MainMenu_Play", (LITERAL_KEY, "Play")),
    ("MainMenu_Options", (LITERAL_KEY, "Settings")),
    ("MainMenu_Exit", (LITERAL_KEY, "Exit")),
    ("MainMenu_Solo", (LITERAL_KEY, "Solo")),
    ("MainMenu_Multiplayer", (LITERAL_KEY, "Multiplayer")),
    ("MainMenu_Back", (LITERAL_KEY, "Back")),
    ("Options_Title", (LITERAL_KEY, "Settings")),
    ("Options_Subtitle", (LITERAL_KEY, "Settings")),
    ("Options_Close", (LITERAL_KEY, "×")),
    ("Options_Unavailable", (OSUDROID_KEY, "multiplayer_room_not_available_beatmap")),
    ("Options_General", (LITERAL_KEY, "General")),
    ("Options_Gameplay", (LANGUAGE_PACK_KEY, "opt_category_gameplay")),
    ("Options_Graphics", (LITERAL_KEY, "Graphics")),
    ("Options_Audio", (LITERAL_KEY, "Audio")),
    ("Options_Library", (LITERAL_KEY, "Library")),
    ("Options_Input", (LITERAL_KEY, "Input")),
    ("Options_Advanced", (LITERAL_KEY, "Advanced")),
    ("Options_Account", (LANGUAGE_PACK_KEY, "opt_category_account")),
    ("Options_RegisterAccount", (LANGUAGE_PACK_KEY, "opt_register_title")),
    ("Options_CheckUpdates", (LANGUAGE_PACK_KEY, "opt_update_title")),
    ("Options_BackupSettings", (LANGUAGE_PACK_KEY, "opt_config_backup_title")),
    ("Options_RestoreSettings", (LANGUAGE_PACK_KEY, "opt_config_backup_restore_title")),
    ("Options_DifficultyAlgorithm", (LANGUAGE_PACK_KEY, "difficulty_algorithm_title")),
    ("Options_Language", (LANGUAGE_PACK_KEY, "opt_language_title")),
    ("Options_CategoryOnline", (LANGUAGE_PACK_KEY, "opt_category_online")),
    ("Options_ServerConnectionTitle", (LANGUAGE_PACK_KEY, "opt_stayonline_title")),
    ("Options_ServerConnectionSummary", (LANGUAGE_PACK_KEY, "opt_stayonline_summary")),
    ("Options_LoadAvatarTitle", (LANGUAGE_PACK_KEY, "opt_loadavatar_title")),
    ("Options_LoadAvatarSummary", (LANGUAGE_PACK_KEY, "opt_loadavatar_summary")),
    ("Options_DifficultyAlgorithmTitle", (LANGUAGE_PACK_KEY, "difficulty_algorithm_title")),
    ("Options_DifficultyAlgorithmSummary", (LANGUAGE_PACK_KEY, "difficulty_algorithm_summary")),
    ("Options_CategoryAccount", (LANGUAGE_PACK_KEY, "opt_category_account")),
    ("Options_LoginTitle", (LANGUAGE_PACK_KEY, "opt_login_title")),
    ("Options_LoginSummary", (LANGUAGE_PACK_KEY, "opt_login_summary")),
    ("Options_PasswordTitle", (LANGUAGE_PACK_KEY, "opt_password_title")),
    ("Options_PasswordSummary", (LANGUAGE_PACK_KEY, "opt_password_summary")),
    ("Options_RegisterTitle", (LANGUAGE_PACK_KEY, "opt_register_title")),
    ("Options_RegisterSummary", (LANGUAGE_PACK_KEY, "opt_register_summary")),
    ("Options_CategoryCommunity", (LANGUAGE_PACK_KEY, "opt_category_community")),
    ("Options_ReceiveAnnouncementsTitle", (LANGUAGE_PACK_KEY, "opt_receive_announcements_title")),
    ("Options_ReceiveAnnouncementsSummary", (LANGUAGE_PACK_KEY, "opt_receive_announcements_summary")),
    ("Options_CategoryUpdates", (LANGUAGE_PACK_KEY, "opt_category_updates")),
    ("Options_UpdateTitle", (LANGUAGE_PACK_KEY, "opt_update_title")),
    ("Options_UpdateSummary", (LANGUAGE_PACK_KEY, "opt_update_summary")),
    ("Options_CategoryConfigBackup", (LANGUAGE_PACK_KEY, "opt_category_config_backup")),
    ("Options_BackupTitle", (LANGUAGE_PACK_KEY, "opt_config_backup_title")),
    ("Options_BackupSummary", (LANGUAGE_PACK_KEY, "opt_config_backup_summary")),
    ("Options_RestoreTitle", (LANGUAGE_PACK_KEY, "opt_config_backup_restore_title")),
    ("Options_RestoreSummary", (LANGUAGE_PACK_KEY, "opt_config_backup_restore_summary")),
    ("Options_CategoryLocalization", (LANGUAGE_PACK_KEY, "opt_category_localization")),
    ("Options_LanguageTitle", (LANGUAGE_PACK_KEY, "opt_language_title")),
    ("Options_LanguageSummary", (LANGUAGE_PACK_KEY, "opt_language_summary")),
    ("Options_DifficultyAlgorithmValue", (LANGUAGE_PACK_KEY, "favorite_default")),
    ("Options_DifficultyAlgorithmDroid", (LITERAL_KEY, "osu!droid")),
    ("Options_DifficultyAlgorithmStandard", (LITERAL_KEY, "osu!standard")),
    ("Options_LanguageValue", (OSUDROID_KEY, "multiplayer_room_chat_system")),
    ("Options_LanguageSystemDefault", (OSUDROID_KEY, "multiplayer_room_chat_system")),
    ("Options_EmptySummary", (LITERAL_KEY, "")),
    ("Options_CategoryHitObjects", (LANGUAGE_PACK_KEY, "opt_category_hit_objects")),
    ("Options_ShowFirstApproachCircleTitle", (LANGUAGE_PACK_KEY, "opt_show_first_approach_circle_title")),
    ("Options_ShowFirstApproachCircleSummary", (LANGUAGE_PACK_KEY, "opt_show_first_approach_circle_summary")),
    ("Options_CategoryBackground", (LANGUAGE_PACK_KEY, "opt_category_background")),
    ("Options_KeepBackgroundAspectRatioTitle", (LANGUAGE_PACK_KEY, "opt_keep_background_aspect_ratio_title")),
    ("Options_KeepBackgroundAspectRatioSummary", (LANGUAGE_PACK_KEY, "opt_keep_background_aspect_ratio_summary")),
    ("Options_EnableStoryboardTitle", (LANGUAGE_PACK_KEY, "opt_enableStoryboard_title")),
    ("Options_EnableStoryboardSummary", (LANGUAGE_PACK_KEY, "opt_enableStoryboard_title")),
    ("Options_EnableVideoTitle", (LANGUAGE_PACK_KEY, "opt_video_title")),
    ("Options_EnableVideoSummary", (LANGUAGE_PACK_KEY, "opt_video_summary")),
    ("Options_CategoryPlayfield", (LANGUAGE_PACK_KEY, "opt_category_playfield")),
    ("Options_DisplayPlayfieldBorderTitle", (LANGUAGE_PACK_KEY, "opt_display_playfield_border_title")),
    ("Options_DisplayPlayfieldBorderSummary", (LANGUAGE_PACK_KEY, "opt_display_playfield_border_summary")),
    ("Options_CategoryHud", (LANGUAGE_PACK_KEY, "opt_category_hud")),
    ("Options_HideInGameUiTitle", (LANGUAGE_PACK_KEY, "opt_hide_ingame_ui_title")),
    ("Options_HideInGameUiSummary", (LANGUAGE_PACK_KEY, "opt_hide_ingame_ui_summary")),
    ("Options_HideReplayMarqueeTitle", (LANGUAGE_PACK_KEY, "opt_hide_replay_marquee_title")),
    ("Options_HideReplayMarqueeSummary", (LANGUAGE_PACK_KEY, "opt_hide_replay_marquee_summary")),
    ("Options_FpsTitle", (LANGUAGE_PACK_KEY, "opt_fps_title")),
    ("Options_FpsSummary", (LANGUAGE_PACK_KEY, "opt_fps_summary")),
    ("Options_DisplayScoreStatisticsTitle", (LANGUAGE_PACK_KEY, "opt_display_score_statistics_title")),
    ("Options_DisplayScoreStatisticsSummary", (LANGUAGE_PACK_KEY, "opt_display_score_statistics_summary")),
    ("Options_CategoryComboColors", (LITERAL_KEY, "Combo Colors")),
    ("Options_ComboColorsTitle", (LANGUAGE_PACK_KEY, "opt_combo_colors_title")),
    ("Options_ComboColorsSummary", (LANGUAGE_PACK_KEY, "opt_combo_colors_summary")),
    ("Options_Combo1Title", (LITERAL_KEY, "Combo 1")),
    ("Options_Combo2Title", (LITERAL_KEY, "Combo 2")),
    ("Options_Combo3Title", (LITERAL_KEY, "Combo 3")),
    ("Options_Combo4Title", (LITERAL_KEY, "Combo 4")),
    ("Options_CategorySkin", (LANGUAGE_PACK_KEY, "opt_category_skin")),
    ("Options_HudEditorTitle", (LANGUAGE_PACK_KEY, "opt_hudEditor_title")),
    ("Options_HudEditorSummary", (LANGUAGE_PACK_KEY, "opt_hudEditor_summary")),
    ("Options_SkinTitle", (LANGUAGE_PACK_KEY, "opt_skin_title")),
    ("Options_SkinSummary", (LANGUAGE_PACK_KEY, "opt_skin_summary")),
    ("Options_SpinnerStyleModern", (LITERAL_KEY, "Modern")),
    ("Options_SpinnerStyleClassical", (LITERAL_KEY, "Classical")),
    ("Options_CategoryCursor", (LANGUAGE_PACK_KEY, "opt_category_cursor")),
    ("Options_ShowCursorTitle", (LANGUAGE_PACK_KEY, "opt_showcursor_title")),
    ("Options_ShowCursorSummary", (LANGUAGE_PACK_KEY, "opt_showcursor_summary")),
    ("Options_ParticlesTitle", (LANGUAGE_PACK_KEY, "opt_particles_title")),
    ("Options_ParticlesSummary", (LANGUAGE_PACK_KEY, "opt_particles_summary")),
    ("Options_CategoryAnimations", (LANGUAGE_PACK_KEY, "opt_category_animations")),
    ("Options_DimHitObjectsTitle", (LANGUAGE_PACK_KEY, "opt_dim_hit_objects_title")),
    ("Options_DimHitObjectsSummary", (LANGUAGE_PACK_KEY, "opt_dim_hit_objects_summary")),
    ("Options_ComboBurstTitle", (LANGUAGE_PACK_KEY, "opt_combo_burst_title")),
    ("Options_ComboBurstSummary", (LANGUAGE_PACK_KEY, "opt_combo_burst_summary")),
    ("Options_LargeImagesTitle", (LANGUAGE_PACK_KEY, "opt_largeimages_title")),
    ("Options_LargeImagesSummary", (LANGUAGE_PACK_KEY, "opt_largeimages_summary")),
    ("Options_AnimateFollowCircleTitle", (LANGUAGE_PACK_KEY, "opt_animate_follow_circle_title")),
    ("Options_AnimateFollowCircleSummary", (LANGUAGE_PACK_KEY, "opt_animate_follow_circle_summary")),
    ("Options_AnimateComboTextTitle", (LANGUAGE_PACK_KEY, "opt_animate_combo_text_title")),
    ("Options_AnimateComboTextSummary", (LANGUAGE_PACK_KEY, "opt_animate_combo_text_summary")),
    ("Options_SnakingInSlidersTitle", (LANGUAGE_PACK_KEY, "opt_snakingInSliders_title")),
    ("Options_SnakingInSlidersSummary", (LANGUAGE_PACK_KEY, "opt_snakingInSliders_summary")),
    ("Options_SnakingOutSlidersTitle", (LANGUAGE_PACK_KEY, "opt_snakingOutSliders_title")),
    ("Options_SnakingOutSlidersSummary", (LANGUAGE_PACK_KEY, "opt_snakingOutSliders_summary")),
    ("Options_NoChangeDimInBreaksTitle", (LANGUAGE_PACK_KEY, "opt_noChangeDimInBreaks_title")),
    ("Options_NoChangeDimInBreaksSummary", (LANGUAGE_PACK_KEY, "opt_noChangeDimInBreaks_summary")),
    ("Options_BurstsTitle", (LANGUAGE_PACK_KEY, "opt_bursts_title")),
    ("Options_BurstsSummary", (LANGUAGE_PACK_KEY, "opt_bursts_summary")),
    ("Options_HitLightingTitle", (LANGUAGE_PACK_KEY, "opt_hitlighting_title")),
    ("Options_HitLightingSummary", (LANGUAGE_PACK_KEY, "opt_hitlighting_summary")),
    ("Options_CategoryVolume", (LANGUAGE_PACK_KEY, "opt_category_volume")),
    ("Options_BgmVolumeTitle", (LANGUAGE_PACK_KEY, "opt_bgm_volume_title")),
    ("Options_BgmVolumeSummary", (LANGUAGE_PACK_KEY, "opt_bgm_volume_summary")),
    ("Options_SoundVolumeTitle", (LANGUAGE_PACK_KEY, "opt_sound_volume_title")),
    ("Options_SoundVolumeSummary", (LANGUAGE_PACK_KEY, "opt_sound_volume_summary")),
    ("Options_CategoryOffset", (LANGUAGE_PACK_KEY, "opt_category_offset")),
    ("Options_OffsetTitle", (LANGUAGE_PACK_KEY, "opt_offset_title")),
    ("Options_OffsetSummary", (LANGUAGE_PACK_KEY, "opt_offset_summary")),
    ("Options_OffsetCalibrationTitle", (OSUDROID_KEY, "opt_offset_calibration_title")),
    ("Options_OffsetCalibrationSummary", (OSUDROID_KEY, "opt_offset_calibration_summary")),
    ("Options_GameAudioSynchronizationThresholdTitle", (LANGUAGE_PACK_KEY, "opt_gameAudioSynchronizationThreshold_title")),
    ("Options_GameAudioSynchronizationThresholdSummary", (LANGUAGE_PACK_KEY, "opt_gameAudioSynchronizationThreshold_summary")),
    ("Options_CategoryEffect", (LANGUAGE_PACK_KEY, "opt_category_effect")),
    ("Options_MetronomeSwitchTitle", (LANGUAGE_PACK_KEY, "opt_metronome_switch_title")),
    ("Options_MetronomeSwitchSummary", (LANGUAGE_PACK_KEY, "opt_metronome_switch_summary")),
    ("Options_MetronomeSwitchValue", (LITERAL_KEY, "All")),
    ("Options_ShiftPitchTitle", (LANGUAGE_PACK_KEY, "opt_shiftPitchInRateChange_title")),
    ("Options_ShiftPitchSummary", (LANGUAGE_PACK_KEY, "opt_shiftPitchInRateChange_summary")),
    ("Options_CategoryMiscellaneous", (LANGUAGE_PACK_KEY, "opt_category_miscellaneous")),
    ("Options_BeatmapSoundsTitle", (LANGUAGE_PACK_KEY, "opt_sound_title")),
    ("Options_BeatmapSoundsSummary", (LANGUAGE_PACK_KEY, "opt_sound_summary")),
    ("Options_MusicPreviewTitle", (LANGUAGE_PACK_KEY, "opt_musicpreview_title")),
    ("Options_MusicPreviewSummary", (LANGUAGE_PACK_KEY, "opt_musicpreview_summary")),
    ("Options_CategoryImport", (LANGUAGE_PACK_KEY, "opt_category_import")),
    ("Options_DeleteOszTitle", (LANGUAGE_PACK_KEY, "opt_deleteosz_title")),
    ("Options_DeleteOszSummary", (LANGUAGE_PACK_KEY, "opt_deleteosz_summary")),
    ("Options_ScanDownloadTitle", (LANGUAGE_PACK_KEY, "opt_scandownload_title")),
    ("Options_ScanDownloadSummary", (LANGUAGE_PACK_KEY, "opt_scandownload_summary")),
    ("Options_DeleteUnimportedTitle", (LANGUAGE_PACK_KEY, "opt_deleteUnimportedBeatmaps_title")),
    ("Options_DeleteUnimportedSummary", (LANGUAGE_PACK_KEY, "opt_deleteUnimportedBeatmaps_summary")),
    ("Options_DeleteUnsupportedVideosTitle", (LANGUAGE_PACK_KEY, "opt_delete_unsupported_videos_title")),
    ("Options_DeleteUnsupportedVideosSummary", (LANGUAGE_PACK_KEY, "opt_delete_unsupported_videos_summary")),
    ("Options_PreferNoVideoDownloadsTitle", (OSUDROID_KEY, "opt_prefer_no_video_downloads_title")),
    ("Options_PreferNoVideoDownloadsSummary", (OSUDROID_KEY, "opt_prefer_no_video_downloads_summary")),
    ("Options_ImportReplayTitle", (LANGUAGE_PACK_KEY, "opt_import_replay_title")),
    ("Options_ImportReplaySummary", (LANGUAGE_PACK_KEY, "opt_import_replay_summary")),
    ("Options_CategoryMetadata", (LANGUAGE_PACK_KEY, "opt_category_metadata")),
    ("Options_ForceRomanizedTitle", (LANGUAGE_PACK_KEY, "force_romanized")),
    ("Options_ForceRomanizedSummary", (LANGUAGE_PACK_KEY, "force_romanized_summary")),
    ("Options_CategoryStorage", (LANGUAGE_PACK_KEY, "opt_category_storage")),
    ("Options_ClearBeatmapCacheTitle", (LANGUAGE_PACK_KEY, "opt_clear_title")),
    ("Options_ClearBeatmapCacheSummary", (LANGUAGE_PACK_KEY, "opt_clear_summary")),
    ("Options_ClearPropertiesTitle", (LANGUAGE_PACK_KEY, "opt_clearprops_title")),
    ("Options_ClearPropertiesSummary", (LANGUAGE_PACK_KEY, "opt_clearprops_summary")),
    ("Options_CategoryGameplay", (LANGUAGE_PACK_KEY, "opt_category_gameplay")),
    ("Options_BlockAreasTitle", (LANGUAGE_PACK_KEY, "block_area_preference_title")),
    ("Options_BlockAreasSummary", (LANGUAGE_PACK_KEY, "block_area_preference_summary")),
    ("Options_HighPrecisionInputTitle", (OSUDROID_KEY, "opt_highPrecisionInput_title")),
    ("Options_HighPrecisionInputSummary", (OSUDROID_KEY, "opt_highPrecisionInput_summary")),
    ("Options_BackButtonPressTimeTitle", (LANGUAGE_PACK_KEY, "opt_backButtonPressTime_title")),
    ("Options_BackButtonPressTimeSummary", (LANGUAGE_PACK_KEY, "opt_backButtonPressTime_summary")),
    ("Options_RemoveSliderLockTitle", (OSUDROID_KEY, "opt_remove_sliderlock_spinnerlock_title")),
    ("Options_RemoveSliderLockSummary", (OSUDROID_KEY, "opt_remove_sliderlock_spinnerlock_summary")),
    ("Options_CategoryVibration", (LITERAL_KEY, "Vibration")),
    ("Options_VibrationCircleTitle", (LITERAL_KEY, "Circle")),
    ("Options_VibrationSliderTitle", (LITERAL_KEY, "Slider")),
    ("Options_VibrationSpinnerTitle", (LITERAL_KEY, "Spinner")),
    ("Options_VibrationSummary", (LANGUAGE_PACK_KEY, "opt_seekBarVibrateIntensity_summary")),
    ("Options_SeekBarVibrateIntensityTitle", (LANGUAGE_PACK_KEY, "opt_seekBarVibrateIntensity_title")),
    ("Options_SeekBarVibrateIntensitySummary", (LANGUAGE_PACK_KEY, "opt_seekBarVibrateIntensity_summary")),
    ("Options_CategorySynchronization", (LANGUAGE_PACK_KEY, "opt_category_synchronization")),
    ("Options_FixFrameOffsetTitle", (LANGUAGE_PACK_KEY, "opt_fix_frame_offset_title")),
    ("Options_FixFrameOffsetSummary", (LANGUAGE_PACK_KEY, "opt_fix_frame_offset_summary")),
    ("Options_CategoryDirectories", (LANGUAGE_PACK_KEY, "opt_category_directories")),
    ("Options_SongDirectoryTitle", (LANGUAGE_PACK_KEY, "opt_directory_title")),
    ("Options_ForceMaxRefreshRateTitle", (LANGUAGE_PACK_KEY, "opt_force_max_refresh_rate_title")),
    ("Options_ForceMaxRefreshRateSummary", (LANGUAGE_PACK_KEY, "opt_force_max_refresh_rate_summary")),
    ("Options_SafeBeatmapBgTitle", (LANGUAGE_PACK_KEY, "opt_safe_beatmap_bg_title")),
    ("Options_SafeBeatmapBgSummary", (LANGUAGE_PACK_KEY, "opt_safe_beatmap_bg_summary")),
    ("Options_CorePathTitle", (LANGUAGE_PACK_KEY, "opt_corepath_title")),
    ("Options_CorePathSummary", (LANGUAGE_PACK_KEY, "opt_corepath_summary")),
    ("Options_CorePathSummaryIos", (LITERAL_KEY, "osu!droid main directory")),
    ("Options_SkinTopPathTitle", (LANGUAGE_PACK_KEY, "opt_skin_top_path_title")),
    ("Options_SkinTopPathSummary", (LANGUAGE_PACK_KEY, "opt_skin_top_path_summary")),
    ("Options_SkinTopPathSummaryIos", (LITERAL_KEY, "Path to directory containing skin files. (default: {0})")),
    ("Options_DirectoryTitle", (LANGUAGE_PACK_KEY, "opt_directory_title")),
    ("Options_DirectorySummary", (LANGUAGE_PACK_KEY, "opt_directory_summary")),
    ("Options_DirectorySummaryIos", (LITERAL_KEY, "Path to directory containing beatmaps (default: {0})")),
    ("Options_DirectoryValue", (LITERAL_KEY, "/sdcard/osu!droid/Songs")),
    ("MainMenu_AboutTitle", (LITERAL_KEY, "About")),
    ("MainMenu_AboutVersion", (LITERAL_KEY, "Version {0}")),
    ("MainMenu_AboutMadeBy", (LITERAL_KEY, "Made by osu!droid team")),
    ("MainMenu_AboutCopyright", (LITERAL_KEY, "osu! is © peppy 2007-2026")),
    ("MainMenu_AboutOsuWebsite", (LITERAL_KEY, "Visit official osu! website ↗")),
    ("MainMenu_AboutOsuDroidWebsite", (LITERAL_KEY, "Visit official osu!droid website ↗")),
    ("MainMenu_AboutDiscord", (LITERAL_KEY, "Join the official Discord server ↗")),
    ("MainMenu_AboutChangelog", (LANGUAGE_PACK_KEY, "changelog_title")),
    ("MainMenu_AboutClose", (OSUDROID_KEY, "multiplayer_room_kicked_close")),
    ("SongSelect_DeleteBeatmapTitle", (LANGUAGE_PACK_KEY, "menu_properties_delete")),
    ("SongSelect_DeleteBeatmapMessage", (LANGUAGE_PACK_KEY, "favorite_ensure")),
    ("SongSelect_PropertiesTitle", (LANGUAGE_PACK_KEY, "menu_properties_title")),
    ("SongSelect_Offset", (LANGUAGE_PACK_KEY, "menu_properties_offset")),
    ("SongSelect_AddToFavorites", (LANGUAGE_PACK_KEY, "menu_properties_tofavs")),
    ("SongSelect_ManageFavorites", (LANGUAGE_PACK_KEY, "favorite_manage")),
    ("SongSelect_SearchPlaceholder", (LANGUAGE_PACK_KEY, "menu_search_filter")),
    ("SongSelect_DefaultFavoriteFolder", (LANGUAGE_PACK_KEY, "favorite_default")),
    ("SongSelect_CreateNewFolder", (LANGUAGE_PACK_KEY, "favorite_new_folder")),
    ("SongSelect_NoCollections", (LITERAL_KEY, "No collections")),
    ("SongSelect_CollectionBeatmaps", (LITERAL_KEY, "· {0} beatmaps")),
    ("Common_Yes", (LANGUAGE_PACK_KEY, "dialog_exit_yes")),
    ("Common_No", (LANGUAGE_PACK_KEY, "dialog_exit_no")),
    ("Common_All", (LITERAL_KEY, "All")),
    ("BeatmapDownloader_SearchInitial", (LANGUAGE_PACK_KEY, "beatmap_downloader_connecting")),
    ("BeatmapDownloader_SearchPlaceholder", (LANGUAGE_PACK_KEY, "menu_search_filter")),
    ("BeatmapDownloader_Filters", (LITERAL_KEY, "Filters")),
    ("BeatmapDownloader_SortBy", (LITERAL_KEY, "Sort by")),
    ("BeatmapDownloader_RankedStatus", (LITERAL_KEY, "Ranked status")),
    ("BeatmapDownloader_Order", (LITERAL_KEY, "Order")),
    ("BeatmapDownloader_Status", (LITERAL_KEY, "Status")),
    ("BeatmapDownloader_SelectMirror", (LITERAL_KEY, "Select a beatmap mirror")),
    ("BeatmapDownloader_Ascending", (LITERAL_KEY, "Ascending")),
    ("BeatmapDownloader_Descending", (LITERAL_KEY, "Descending")),
    ("BeatmapDownloader_Download", (LITERAL_KEY, "Download")),
    ("BeatmapDownloader_DownloadNoVideo", (LITERAL_KEY, "Download (no video)")),
    ("BeatmapDownloader_NoVideo", (LITERAL_KEY, "No video")),
    ("BeatmapDownloader_LoadingMore", (LITERAL_KEY, "Loading more...")),
    ("BeatmapDownloader_NoBeatmapsFound", (LITERAL_KEY, "No beatmaps found")),
    ("BeatmapDownloader_Searching", (LITERAL_KEY, "Searching {0}...")),
    ("BeatmapDownloader_MappedBy", (LITERAL_KEY, "Mapped by {0}")),
    ("BeatmapDownloader_Connecting", (LANGUAGE_PACK_KEY, "beatmap_downloader_connecting")),
    ("BeatmapDownloader_Downloading", (LANGUAGE_PACK_KEY, "beatmap_downloader_downloading")),
    ("BeatmapDownloader_Importing", (LANGUAGE_PACK_KEY, "beatmap_downloader_importing")),
    ("BeatmapDownloader_Beatmap", (LITERAL_KEY, "beatmap")),
    ("BeatmapDownloader_Ranked", (LANGUAGE_PACK_KEY, "ranked_status_ranked")),
    ("BeatmapDownloader_Approved", (LANGUAGE_PACK_KEY, "ranked_status_approved")),
    ("BeatmapDownloader_Qualified", (LANGUAGE_PACK_KEY, "ranked_status_qualified")),
    ("BeatmapDownloader_Loved", (LANGUAGE_PACK_KEY, "ranked_status_loved")),
    ("BeatmapDownloader_WorkInProgress", (LANGUAGE_PACK_KEY, "ranked_status_wip")),
    ("BeatmapDownloader_Graveyard", (LANGUAGE_PACK_KEY, "ranked_status_graveyard")),
    ("BeatmapDownloader_Pending", (LANGUAGE_PACK_KEY, "ranked_status_pending")),
    ("Sort_Title", (LANGUAGE_PACK_KEY, "menu_search_sort_title")),
    ("Sort_Artist", (LANGUAGE_PACK_KEY, "menu_search_sort_artist")),
    ("Sort_Creator", (LANGUAGE_PACK_KEY, "menu_search_sort_creator")),
    ("Sort_Date", (LANGUAGE_PACK_KEY, "menu_search_sort_date")),
    ("Sort_Bpm", (LANGUAGE_PACK_KEY, "menu_search_sort_bpm")),
    ("Sort_DroidStars", (LANGUAGE_PACK_KEY, "menu_search_sort_droid_stars")),
    ("Sort_StandardStars", (LANGUAGE_PACK_KEY, "menu_search_sort_standard_stars")),
    ("Sort_Length", (LANGUAGE_PACK_KEY, "menu_search_sort_length")),
    ("Sort_DifficultyRating", (LITERAL_KEY, "Difficulty rating")),
    ("Sort_HitLength", (LITERAL_KEY, "Hit length")),
    ("Sort_PassCount", (LITERAL_KEY, "Pass count")),
    ("Sort_PlayCount", (LITERAL_KEY, "Play count")),
    ("Sort_TotalLength", (LITERAL_KEY, "Total length")),
    ("Sort_FavouriteCount", (LITERAL_KEY, "Favourite count")),
    ("Sort_LastUpdated", (LITERAL_KEY, "Last updated")),
    ("Sort_RankedDate", (LITERAL_KEY, "Ranked date")),
    ("Sort_SubmittedDate", (LITERAL_KEY, "Submitted date")),
    ("SongSelect_RemoveCollectionTitle", (LITERAL_KEY, "Remove collection")),
    ("BeatmapDownloader_ConnectionFailed", (LITERAL_KEY, "Failed to connect to server, please check your internet connection.")),
    ("BeatmapDownloader_Downloaded", (LITERAL_KEY, "Beatmap downloaded")),
    ("Common_Cancel", (LANGUAGE_PACK_KEY, "beatmap_downloader_cancel")),
    ("BeatmapDownloader_Cancel", (LANGUAGE_PACK_KEY, "beatmap_downloader_cancel")),
    ("SongSelect_Creator", (LANGUAGE_PACK_KEY, "menu_creator")),
    ("SongSelect_DifficultyStats", (LANGUAGE_PACK_KEY, "binfoStr1")),
    ("SongSelect_ObjectStats", (LANGUAGE_PACK_KEY, "binfoStr2")),
    ("MainMenu_ExitInstruction", (LITERAL_KEY, "Done playing? Swipe this app away to close it.")),
    ("MainMenu_ExitDialogTitle", (LITERAL_KEY, "Exit")),
    ("MainMenu_ExitDialogMessage", (LANGUAGE_PACK_KEY, "dialog_exit_message")),
    ("MainMenu_ExitDialogConfirm", (LITERAL_KEY, "Yes")),
    ("MainMenu_ExitDialogCancel", (LITERAL_KEY, "No")),
    ("MainMenu_DevelopmentBuild", (LITERAL_KEY, "DEVELOPMENT BUILD")),
    ("SongSelect_BeatmapBy", (LITERAL_KEY, "Beatmap by {0}")),
    ("SongSelect_DifficultyAdvancedStats", (LITERAL_KEY, "AR: {0} OD: {1} CS: {2} HP: {3} Stars: {4}")),
    ("BeatmapDownloader_DetailsStats", (LITERAL_KEY, "Star rating: {0}\nAR: {1} - OD: {2} - CS: {3} - HP: {4}\nCircles: {5} - Sliders: {6} - Spinners: {7}\nLength: {8} - BPM: {9}")),
])


def element_text(element: ET.Element) -> str:
    if element.tag == "string":
        raw = "".join(element.itertext())
    elif element.tag == "string-array":
        raw = "|".join("".join(item.itertext()) for item in element.findall("item"))
    elif element.tag == "plurals":
        raw = "|".join(f"{item.attrib.get('quantity')}={''.join(item.itertext())}" for item in element.findall("item"))
    else:
        raw = ""

    return android_to_resx_format(android_unescape(raw.strip()))


def android_unescape(value: str) -> str:
    return (
        html.unescape(value)
        .replace("\\'", "'")
        .replace('\\"', '"')
        .replace("\\n", "\n")
    )


def android_to_resx_format(value: str) -> str:
    index = 0

    def replace(_: re.Match[str]) -> str:
        nonlocal index
        current = index
        index += 1
        return "{" + str(current) + "}"

    return re.sub(r"%(?:\d+\$)?[sd]", replace, value)


def read_values(root: Path, english_only: bool) -> OrderedDict[str, str]:
    values: OrderedDict[str, str] = OrderedDict()

    if not root.exists():
        raise FileNotFoundError(root)

    for path in sorted(root.rglob("res/values*/*.xml")):
        if english_only and path.parent.name != "values":
            continue

        document = ET.parse(path)
        for element in document.getroot():
            if element.tag not in {"string", "string-array", "plurals"}:
                continue

            name = element.attrib.get("name")
            if name:
                values[name] = element_text(element)

    return values


def resolve_alias(kind: str, source: str, osudroid: OrderedDict[str, str], language_pack: OrderedDict[str, str]) -> str:
    if kind == OSUDROID_KEY:
        return osudroid[source]

    if kind == LANGUAGE_PACK_KEY:
        return language_pack[source]

    if kind == LITERAL_KEY:
        return source

    raise ValueError(f"unknown alias kind: {kind}")


def generate_entries() -> OrderedDict[str, str]:
    osudroid = read_values(OSUDROID_SOURCE, english_only=True)
    language_pack = read_values(LANGUAGE_PACK, english_only=True)
    entries: OrderedDict[str, str] = OrderedDict()

    for name, (kind, source) in APP_ALIASES.items():
        entries[name] = resolve_alias(kind, source, osudroid, language_pack)

    for name, value in osudroid.items():
        entries[f"OsuDroid_{name}"] = value

    for name, value in language_pack.items():
        entries[f"OsuDroidLanguagePack_{name}"] = value

    return entries


def render_resx(entries: OrderedDict[str, str]) -> str:
    lines = [
        '<?xml version="1.0" encoding="utf-8"?>',
        "<root>",
        '    <resheader name="resmimetype">',
        "        <value>text/microsoft-resx</value>",
        "    </resheader>",
        '    <resheader name="version">',
        "        <value>2.0</value>",
        "    </resheader>",
        '    <resheader name="reader">',
        "        <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>",
        "    </resheader>",
        '    <resheader name="writer">',
        "        <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>",
        "    </resheader>",
    ]

    for name, value in entries.items():
        lines.extend([
            f'    <data name="{html.escape(name, quote=True)}"',
            '          xml:space="preserve">',
            f"        <value>{html.escape(value, quote=False)}</value>",
            "    </data>",
        ])

    lines.append("</root>")
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate English localization from osu!droid Android sources.")
    parser.add_argument("--check", action="store_true", help="verify Strings.resx is up to date")
    args = parser.parse_args()

    generated = render_resx(generate_entries())

    if args.check:
        current = RESX.read_text()
        if current != generated:
            print("Strings.resx is out of date. Run scripts/dev/generate-osudroid-localization.py", file=sys.stderr)
            return 1
        current_template = LOCALE_TEMPLATE.read_text()
        if current_template != generated:
            print("Strings.locale.resx.template is out of date. Run scripts/dev/generate-osudroid-localization.py", file=sys.stderr)
            return 1
        return 0

    RESX.write_text(generated)
    LOCALE_TEMPLATE.write_text(generated)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
