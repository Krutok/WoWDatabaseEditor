using System;
using WDE.Common.Services;
using WDE.Module.Attributes;

namespace WDE.PacketViewer.Processing.Processors.ActionReaction
{
    public interface IActionReactionProcessorCreator
    {
        ActionReactionProcessor Create();
        ActionReactionToTextProcessor CreateTextProcessor();
    }
    
    [AutoRegister]
    [SingleInstance]
    public class ActionReactionProcessorCreator : IActionReactionProcessorCreator
    {
        private readonly ISpellService spellService;
        private readonly Func<RandomMovementDetector> random;
        private readonly Func<IUnitPositionFollower> unitFollower;
        private readonly Func<IChatEmoteSoundProcessor> chatEmote;
        private readonly Func<IWaypointProcessor> waypointProcessor;
        private readonly Func<IUpdateObjectFollower> update;
        private readonly Func<IPlayerGuidFollower> player;
        private readonly Func<IAuraSlotTracker> auraSlotTracker;

        public ActionReactionProcessorCreator(
            ISpellService spellService,
            Func<RandomMovementDetector> random,
            Func<IUnitPositionFollower> unitFollower,
            Func<IChatEmoteSoundProcessor> chatEmote,
            Func<IWaypointProcessor> waypointProcessor,
            Func<IUpdateObjectFollower> update,
            Func<IPlayerGuidFollower> player,
            Func<IAuraSlotTracker> auraSlotTracker)
        {
            this.spellService = spellService;
            this.random = random;
            this.unitFollower = unitFollower;
            this.chatEmote = chatEmote;
            this.waypointProcessor = waypointProcessor;
            this.update = update;
            this.player = player;
            this.auraSlotTracker = auraSlotTracker;
        }
        
        public ActionReactionProcessor Create()
        {
            var unitFollower = this.unitFollower();
            var chatEmote = this.chatEmote();
            var random = this.random();
            var waypointProcessor = this.waypointProcessor();
            var update = this.update();
            var player = this.player();
            var auraSlotTracker = this.auraSlotTracker();
            return new ActionReactionProcessor(
                new ActionGenerator(spellService, unitFollower, chatEmote, update, player, waypointProcessor, auraSlotTracker),
                new EventDetectorProcessor(spellService, unitFollower, random, chatEmote, waypointProcessor, update,
                    player, auraSlotTracker),
                random,
                chatEmote,
                waypointProcessor,
                unitFollower,
                update, player, auraSlotTracker);
        }
        
        public ActionReactionToTextProcessor CreateTextProcessor()
        {
            var unitFollower = this.unitFollower();
            var chatEmote = this.chatEmote();
            var random = this.random();
            var waypointProcessor = this.waypointProcessor();
            var update = this.update();
            var player = this.player();
            var auraSlotTracker = this.auraSlotTracker();
            return new ActionReactionToTextProcessor(
                new ActionGenerator(spellService, unitFollower, chatEmote, update, player, waypointProcessor, auraSlotTracker),
                new EventDetectorProcessor(spellService, unitFollower, random, chatEmote, waypointProcessor, update,
                    player, auraSlotTracker),
                random,
                chatEmote,
                waypointProcessor,
                unitFollower,
                update, player,
                auraSlotTracker);
        }
    }
}