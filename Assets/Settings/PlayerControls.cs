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
    readonly InputAction pause;

    public PlayerControls()
    {
        var source = Resources.Load<InputActionAsset>("PlayerControls");
        if (source == null)
            throw new InvalidOperationException("Missing Resources/PlayerControls.inputactions");
        asset = InputActionAsset.FromJson(source.ToJson());
        map = asset.FindActionMap("Player", true);
        move = map.FindAction("Move", true);
        jump = map.FindAction("Jump", true);
        interact = map.FindAction("Interact", true);
        pause = map.FindAction("Pause", true);
    }

    public PlayerActions Player => new(this);

    public void Enable() => asset.Enable();
    public void Disable() => asset.Disable();

    public void Dispose()
    {
        if (asset == null)
            return;
        asset.Disable();
        UnityEngine.Object.Destroy(asset);
    }

    public struct PlayerActions
    {
        readonly PlayerControls wrapper;

        public PlayerActions(PlayerControls wrapper) => this.wrapper = wrapper;

        public InputAction Move => wrapper.move;
        public InputAction Jump => wrapper.jump;
        public InputAction Interact => wrapper.interact;
        public InputAction Pause => wrapper.pause;
    }
}
