package moe.osudroid.ui;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.TextureRegion;
import com.badlogic.gdx.scenes.scene2d.ui.CheckBox;
import com.badlogic.gdx.scenes.scene2d.ui.Label;
import com.badlogic.gdx.scenes.scene2d.ui.ScrollPane;
import com.badlogic.gdx.scenes.scene2d.ui.Skin;
import com.badlogic.gdx.scenes.scene2d.ui.TextButton;
import com.badlogic.gdx.scenes.scene2d.utils.TextureRegionDrawable;

import moe.osudroid.assets.ui.UiPalette;
import moe.osudroid.assets.ui.UiResourceCatalog;

public final class UiSkinFactory {
    private UiSkinFactory() {
    }

    public static Skin create(UiResourceCatalog catalog, Texture chromePixel) {
        UiPalette palette = catalog.getManifest().getPalette();
        Skin skin = new Skin();
        BitmapFont font = new BitmapFont();
        TextureRegionDrawable base = new TextureRegionDrawable(new TextureRegion(chromePixel));

        skin.add("font-default", font);
        skin.add("panel", tinted(base, palette.getPanel()));
        skin.add("panel-alt", tinted(base, palette.getPanelAlt()));
        skin.add("accent-fill", tinted(base, palette.getAccent()));
        skin.add("accent-soft-fill", tinted(base, palette.getAccentSoft()));

        Label.LabelStyle labelStyle = new Label.LabelStyle(font, parseColor(palette.getTextPrimary()));
        skin.add("default", labelStyle);

        Label.LabelStyle titleStyle = new Label.LabelStyle(font, parseColor(palette.getAccent()));
        skin.add("title", titleStyle);

        TextButton.TextButtonStyle buttonStyle = new TextButton.TextButtonStyle();
        buttonStyle.font = font;
        buttonStyle.fontColor = parseColor(palette.getTextPrimary());
        buttonStyle.up = skin.getDrawable("panel");
        buttonStyle.over = skin.getDrawable("panel-alt");
        buttonStyle.down = skin.getDrawable("accent-soft-fill");
        buttonStyle.checked = skin.getDrawable("accent-fill");
        skin.add("default", buttonStyle);

        TextButton.TextButtonStyle accentButtonStyle = new TextButton.TextButtonStyle(buttonStyle);
        accentButtonStyle.up = skin.getDrawable("accent-fill");
        accentButtonStyle.over = skin.getDrawable("accent-soft-fill");
        accentButtonStyle.down = skin.getDrawable("panel");
        skin.add("accent", accentButtonStyle);

        CheckBox.CheckBoxStyle checkBoxStyle = new CheckBox.CheckBoxStyle();
        checkBoxStyle.checkboxOff = skin.getDrawable("panel");
        checkBoxStyle.checkboxOn = skin.getDrawable("accent-fill");
        checkBoxStyle.font = font;
        checkBoxStyle.fontColor = parseColor(palette.getTextPrimary());
        skin.add("default", checkBoxStyle);

        ScrollPane.ScrollPaneStyle scrollPaneStyle = new ScrollPane.ScrollPaneStyle();
        scrollPaneStyle.background = skin.getDrawable("panel");
        scrollPaneStyle.hScrollKnob = skin.getDrawable("accent-soft-fill");
        scrollPaneStyle.vScrollKnob = skin.getDrawable("accent-soft-fill");
        skin.add("default", scrollPaneStyle);

        return skin;
    }

    private static TextureRegionDrawable tinted(TextureRegionDrawable drawable, String hex) {
        TextureRegionDrawable copy = new TextureRegionDrawable(drawable);
        copy.tint(parseColor(hex));
        return copy;
    }

    private static Color parseColor(String hex) {
        return Color.valueOf(hex.replace("#", ""));
    }
}
