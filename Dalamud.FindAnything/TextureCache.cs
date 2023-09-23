﻿using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;

namespace Dalamud.FindAnything
{
    public class TextureCache : IDisposable
    {
        private readonly ITextureProvider textureProvider;

        public IReadOnlyDictionary<uint, TextureWrap> MainCommandIcons { get; }
        public IReadOnlyDictionary<uint, TextureWrap> GeneralActionIcons { get; }
        public IReadOnlyDictionary<uint, TextureWrap> ContentTypeIcons { get; }
        public IReadOnlyDictionary<uint, TextureWrap> EmoteIcons { get; }
        public IReadOnlyDictionary<uint, TextureWrap> ClassJobIcons { get; }
        public IReadOnlyDictionary<uint, TextureWrap> MountIcons { get; }
        public IReadOnlyDictionary<uint, TextureWrap> MinionIcons { get; }

        public Dictionary<uint, TextureWrap> ExtraIcons { get; }

        public TextureWrap AetheryteIcon { get; }
        public TextureWrap WikiIcon { get; }
        public TextureWrap PluginInstallerIcon { get; }
        public TextureWrap LogoutIcon { get; }
        public TextureWrap EmoteIcon { get; }
        public TextureWrap HintIcon { get; }
        public TextureWrap ChatIcon { get; }
        public TextureWrap MathsIcon { get; }

        public TextureWrap GameIcon { get; }

        private TextureWrap? GetIconTexture(uint iconId)
        {
            var path = textureProvider.GetIconPath(iconId, ITextureProvider.IconFlags.None);
            if (path == null)
                return null;

            return textureProvider.GetTextureFromGame(path);
        }

        private TextureCache(IDataManager data, ITextureProvider textureProvider)
        {
            this.textureProvider = textureProvider;

            var mainCommands = new Dictionary<uint, TextureWrap>();
            foreach (var mainCommand in data.GetExcelSheet<MainCommand>()!)
            {
                mainCommands.Add(mainCommand.RowId, GetIconTexture((uint) mainCommand.Icon)!);
            }
            MainCommandIcons = mainCommands;

            var generalActions = new Dictionary<uint, TextureWrap>();
            foreach (var action in data.GetExcelSheet<GeneralAction>()!)
            {
                generalActions.Add(action.RowId, GetIconTexture((uint) action.Icon)!);
            }
            GeneralActionIcons = generalActions;

            var contentTypes = new Dictionary<uint, TextureWrap>();
            foreach (var cType in data.GetExcelSheet<ContentType>()!)
            {
                if (cType.Icon == 0)
                    continue;

                contentTypes.Add(cType.RowId, GetIconTexture(cType.Icon)!);
            }
            ContentTypeIcons = contentTypes;

            var emotes = new Dictionary<uint, TextureWrap>();
            foreach (var emote in data.GetExcelSheet<Emote>()!)
            {
                var icon = GetIconTexture(emote.Icon);
                if (icon == null)
                    continue;

                emotes.Add(emote.RowId, icon);
            }
            EmoteIcons = emotes;

            var cjIcons = new Dictionary<uint, TextureWrap>();
            foreach (var classJob in data.GetExcelSheet<ClassJob>()!)
            {
                TextureWrap? icon;
                if (classJob.JobIndex != 0)
                {
                    icon = GetIconTexture(062400 + (uint)classJob.JobIndex);
                }
                else
                {
                    var offset = classJob.RowId switch
                    {
                        1 => 1,
                        2 => 2,
                        3 => 3,
                        4 => 4,
                        5 => 5,
                        6 => 6,
                        7 => 7,
                        8 => 10,
                        9 => 11,
                        10 => 12,
                        11 => 13,
                        12 => 14,
                        13 => 15,
                        14 => 16,
                        15 => 17,
                        16 => 18,
                        17 => 19,
                        18 => 20,

                        26 => 8,
                        29 => 9,
                        _ => 0,
                    };

                    icon = GetIconTexture(062300 + (uint) offset);
                }

                if (icon != null)
                    cjIcons.Add(classJob.RowId, icon);
            }
            ClassJobIcons = cjIcons;
            FindAnythingPlugin.Log.Information(ClassJobIcons.Count + " class jobs loaded.");

            var mountIcons = new Dictionary<uint, TextureWrap>();
            foreach (var mount in data.GetExcelSheet<Mount>()!)
            {
                var icon = GetIconTexture(mount.Icon);
                if (icon == null)
                    continue;

                mountIcons.Add(mount.RowId, icon);
            }
            MountIcons = mountIcons;

            var minionIcons = new Dictionary<uint, TextureWrap>();
            foreach (var minion in data.GetExcelSheet<Companion>()!)
            {
                var icon = GetIconTexture(minion.Icon);
                if (icon == null)
                    continue;

                minionIcons.Add(minion.RowId, icon);
            }
            MinionIcons = minionIcons;

            AetheryteIcon = GetIconTexture(066417)!;
            WikiIcon = GetIconTexture(066404)!;
            PluginInstallerIcon = GetIconTexture(066472)!;
            LogoutIcon = GetIconTexture(066403)!;
            EmoteIcon = GetIconTexture(066420)!;
            HintIcon = GetIconTexture(066453)!;
            ChatIcon = GetIconTexture(066473)!;
            MathsIcon = GetIconTexture(062409)!;

            ExtraIcons = new Dictionary<uint, TextureWrap>();

            var gameIconPath = new FileInfo(Path.Combine(
                FindAnythingPlugin.PluginInterface.AssemblyLocation.Directory!.FullName, "noses", "Normal.png"));
            GameIcon = textureProvider.GetTextureFromFile(gameIconPath)!;

            ReloadMacroIcons();
        }

        public void ReloadMacroIcons()
        {
            foreach (var macroLink in FindAnythingPlugin.Configuration.MacroLinks)
            {
                EnsureExtraIcon((uint) macroLink.IconId);
            }
        }

        public void EnsureExtraIcon(uint iconId)
        {
            if (ExtraIcons.ContainsKey(iconId))
                return;

            var tex = GetIconTexture(iconId);

            if (tex != null)
                ExtraIcons[iconId] = tex;
        }

        public static TextureCache Load(IDataManager data, ITextureProvider textureProvider) => new(data, textureProvider);

        public void Dispose()
        {
            foreach (var icon in MainCommandIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in GeneralActionIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in ContentTypeIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in EmoteIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in ExtraIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in ClassJobIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in MountIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in MinionIcons)
            {
                icon.Value.Dispose();
            }

            WikiIcon.Dispose();
            AetheryteIcon.Dispose();
            PluginInstallerIcon.Dispose();
            LogoutIcon.Dispose();
            EmoteIcon.Dispose();
            HintIcon.Dispose();
            ChatIcon.Dispose();
            MathsIcon.Dispose();
            GameIcon.Dispose();
        }
    }
}