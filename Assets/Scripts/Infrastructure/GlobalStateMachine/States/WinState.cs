﻿using KasherOriginal.Factories.UIFactory;

namespace KasherOriginal.GlobalStateMachine
{
    public class WinState : State<GameInstance>
    {
        public WinState(GameInstance context, IUIFactory uiFactory) : base(context)
        {
            _uiFactory = uiFactory;
        }

        private readonly IUIFactory _uiFactory;
        
        
        public override void Enter()
        {
            ShowUI();
        }

        private void ShowUI()
        {
            _uiFactory.CreateGameLoseScreen();
        }
        
        private void HideUI()
        {
            _uiFactory.DestroyGameLoseScreen();
        }
    }
}
