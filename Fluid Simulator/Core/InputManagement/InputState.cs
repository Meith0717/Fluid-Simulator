﻿
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace StellarLiberation.Game.Core.CoreProceses.InputManagement
{
    public enum ActionType
    {
        CameraZoomIn,
        CameraZoomOut,
        MoveCameraLeft,
        MoveCameraRight,
        MoveCameraUp,
        MoveCameraDown,

        // Mouse
        LeftReleased,
        RightJustClicked,
        LeftClickHold,
        RightClickHold,
        LeftWasClicked,
        RightWasClicked,
        MouseWheelForward,
        MouseWheelBackward,
        MoveButtonUp,
        MoveButtonDown,
    }

    public enum KeyEventType
    {
        OnButtonDown,
        OnButtonPressed
    }

    public struct InputState
    {
        public List<ActionType> Actions = new();
        public Vector2 mMousePosition = Vector2.Zero;

        public InputState(List<ActionType> actions, Vector2 mousePosition)
        {
            Actions = actions;
            mMousePosition = mousePosition;
        }

        public readonly bool HasAction(ActionType action) => Actions.Remove(action);

        public readonly void DoAction(ActionType action, Action funktion)
        {
            if (funktion is null) return;
            if (HasAction(action)) funktion();
        }
    }
}
