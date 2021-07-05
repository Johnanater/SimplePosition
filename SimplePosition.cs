using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using SimplePosition.Models;
using UnityEngine;

[assembly: PluginMetadata("SimplePosition", DisplayName = "SimplePosition", Author = "Johnanater", Website = "https://johnanater.com")]
namespace SimplePosition
{
    public class SimplePosition : OpenModUnturnedPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<SimplePosition> m_Logger;

        public Config Config;
        public Color DefaultColor;

        public SimplePosition(
            IConfiguration configuration, 
            IStringLocalizer stringLocalizer,
            ILogger<SimplePosition> logger, 
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
        }

        protected override async UniTask OnLoadAsync()
        {
            Config = m_Configuration.Get<Config>();
            DefaultColor = ParseColor(Config.ChatConfig.MessageColor);

            m_Logger.LogInformation($"Successfully loaded {GetType()} by Johnanater, version {Version}");
        }

        protected override async UniTask OnUnloadAsync()
        {
            m_Logger.LogInformation(m_StringLocalizer[$"Successfully unloaded {GetType()} by Johnanater, version {Version}"]);
        }
        
        //
        // Chat stuff
        //

        public async Task TellAsync(ICommandActor actor, string message, Color color)
        {
            if (!Thread.CurrentThread.IsGameThread())
                await UniTask.SwitchToMainThread();
            Tell(actor, message, color);
        }

        public void Tell(ICommandActor actor, string message, Color color)
        {
            if (string.IsNullOrEmpty(message))
                return;
            
            if (actor is UnturnedUser untUser)
                Tell(untUser.Player.SteamPlayer, message, color);
            else
                actor.PrintMessageAsync(message);
        }

        public void Tell(SteamPlayer player, string message, Color color)
        {
            ChatManager.serverSendMessage(message, color, iconURL: Config.ChatConfig.MessageIconUrl, toPlayer: player, useRichTextFormatting: Config.ChatConfig.UseRichText);
        }

      // Parse color from as an html color
        public Color ParseColor(string str)
        {
            if (ColorUtility.TryParseHtmlString(str, out var color))
                return color;

            m_Logger.LogError($"Cannot parse color '{str}', please fix this in your config!");
            m_Logger.LogError("You can find out how to specify colors here: https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html");
            return Color.green;
        }
    }
}
