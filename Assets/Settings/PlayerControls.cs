using System;
using UnityEngine.InputSystem;

public class PlayerControls : IDisposable
{
    readonly InputActionAsset asset;
    readonly InputActionMap map;
    readonly InputAction move;
    readonly InputAction jump;

    public PlayerControls()
    {
        asset = InputActionAsset.FromJson(System.IO.File.ReadAllText(
            UnityEngine.Application.dataPath + "/Settings/PlayerControls.inputactions"));
        map = asset.FindActionMap("Player", true);
        move = map.FindAction("Move", true);
        jump = map.FindAction("Jump", true);
    }

    public PlayerActions Player => new(this);

    public void Enable() => map.Enable();
    public void Disable() => map.Disable();
    public void Dispose() => map.Disable();

    public struct PlayerActions
    {
        readonly PlayerControls wrapper;

        public PlayerActions(PlayerControls wrapper) => this.wrapper = wrapper;

        public InputAction Move => wrapper.move;
        public InputAction Jump => wrapper.jump;

        public void Enable() => wrapper.Enable();
        public void Disable() => wrapper.Disable();
    }
}