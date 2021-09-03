﻿using System;
using System.Linq;
using Dalamud.Divination.Common.Api.Command;
using Dalamud.Divination.Common.Api.Ui.Window;
using Dalamud.Divination.Common.Boilerplate;
using Dalamud.Divination.Common.Boilerplate.Features;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;

namespace Divination.Voiceroid2Talker
{
    public class Voiceroid2TalkerPlugin : DivinationPlugin<Voiceroid2TalkerPlugin, PluginConfig>,
        IDalamudPlugin, ICommandSupport, IConfigWindowSupport<PluginConfig>
    {
        public Voiceroid2TalkerPlugin(DalamudPluginInterface pluginInterface) : base(pluginInterface)
        {
            Dalamud.ChatGui.ChatMessage += OnChatReceived;
        }

        public string MainCommandPrefix => "/v2t";
        public ConfigWindow<PluginConfig> CreateConfigWindow() => new PluginConfigWindow();

        [Command("/talkv2", "text", Help = "与えられた <text> を読み上げます。", Strict = false)]
        private void OnTalkCommand(CommandContext context)
        {
            Divination.Voiceroid2Proxy.TalkAsync(context.ArgumentText);
        }

        private void OnChatReceived(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            try
            {
                if (Config.EnableTtsFcChatOnInactive && type == XivChatType.FreeCompany)
                {
                    TtsFcChat(sender, message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while OnChatReceived");
            }
        }

        private void TtsFcChat(SeString sender, SeString message)
        {
            if (Win32Api.IsGameClientActive())
            {
                return;
            }

            var senderText = string.Join("", sender.Payloads.Select(x =>
            {
                switch (x)
                {
                    case ITextProvider textProvider:
                        return textProvider.Text;
                    case IconPayload _:
                        return " ";
                    default:
                        return string.Empty;
                }
            }));

            Divination.Voiceroid2Proxy.TalkAsync($"{senderText}: {message.TextValue}");
        }

        protected override void ReleaseManaged()
        {
            Dalamud.ChatGui.ChatMessage -= OnChatReceived;
        }
    }
}
