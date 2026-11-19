using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : IDisposable
{
    readonly InputActionAsset asset;
    readonly InputActionMap map;
    readonly InputAction move;
    readonly InputAction jump;
    readonly InputAction interact;

    public PlayerControls()
    {
        var source = Resources.Load<InputActionAsset>("PlayerControls");
        if (source == null)
            throw new InvalidOperationException("Missing Resources/PlayerControls.inputactions");
        asset = UnityEngine.Object.Instantiate(source);
        map = asset.FindActionMap("Player", true);
        move = map.FindAction("Move", true);
        jump = map.FindAction("Jump", true);
        interact = map.FindAction("Interact", true);
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
        public InputAction Interact => wrapper.interact;
    }
}