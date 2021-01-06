using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;

namespace SimplePosition.Commands
{
    [Command("position")]
    [CommandAlias("pos")]
    [CommandSyntax("[player]")]
    [CommandDescription("Show position")]
    [CommandActor(typeof(UnturnedUser))]
    public class CommandPosition : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly SimplePosition m_SimplePosition;
        
        public CommandPosition(IStringLocalizer stringLocalizer,
            SimplePosition simplePosition,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_SimplePosition = simplePosition;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var untUser = (UnturnedUser) Context.Actor;
            Player targetPlayer = null;
            
            if (Context.Parameters.TryGet<string>(0, out var targetName))
                targetPlayer = PlayerTool.getPlayer(targetName);

            var pos = targetPlayer != null
                ? targetPlayer.transform.position
                : untUser.Player.Player.transform.position;
            var prefix = targetPlayer != null
                ? m_StringLocalizer["their_prefix", new { Username = targetPlayer.channel.owner.playerID.playerName }]
                : m_StringLocalizer["your_prefix"];
            
            await m_SimplePosition.TellAsync(untUser, m_StringLocalizer["position_message", new { Prefix = prefix, Position = pos } ], m_SimplePosition.DefaultColor);
        }
    }
}
