using Godot;
using Godot.Collections;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2MeshInstance3D : MeshInstance3D
{
    [Export] public Array<KH2TextureAnimation> TextureAnimations = new();

    public KH2TextureAnimationFrame CurrentFrame(int index)
    {
        var anim = TextureAnimations[index];
        return anim.AnimationList[anim.CurrentAnimation].Frames[anim.CurrentAnimationFrame];
    }
    public void SetTextureAnimationFrame(int index, int frame)
    {
        if (index > TextureAnimations.Count) return;
        var anim = TextureAnimations[index];
        if (frame >= anim.SpriteFrameCount) return;
        var pos = anim.GetMaterialFrameParameter(frame);
        SetInstanceShaderParameter($"TextureFrame{index}", pos);
    }
    public override void _Ready()
    {
        base._Ready();
        //GD.Print(TextureAnimations.Count);
        for (var index = 0; index < TextureAnimations.Count; index++) SetTextureAnimationFrame(index, 0);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        UpdateAnimatedTextures((float)delta);
    }
    private void SetAnimationStateFrame(int index, int frame)
    {
        var anim = TextureAnimations[index];

        anim.AnimationTimer = 0;
        anim.RandomAnimationTime = -1;
        anim.CurrentAnimationFrame = frame;
        
        var currentFrame = CurrentFrame(index);
        switch (currentFrame.Operation)
        {
            case KH2TextureAnimationOperation.EnableSprite:
                SetInstanceShaderParameter($"UseSprite{index}", true);
                SetTextureAnimationFrame(index, currentFrame.ImageIndex);
                break;
            case KH2TextureAnimationOperation.DisableSprite:
                SetInstanceShaderParameter($"UseSprite{index}", false);
                break;
            case KH2TextureAnimationOperation.Jump:
                SetAnimationStateFrame(index, anim.CurrentAnimationFrame + currentFrame.JumpDelta);
                break;
        }
    }
    public void UpdateAnimatedTextures(float delta)
    {
        for (var index = 0; index < TextureAnimations.Count; index++)
        {
            var anim = TextureAnimations[index];
            var currentFrame = CurrentFrame(index);
            if (currentFrame.Operation == KH2TextureAnimationOperation.Stop) continue;
            if (anim.RandomAnimationTime == -1) anim.RandomAnimationTime = (float)GD.RandRange(currentFrame.MinTime, currentFrame.MaxTime);
            anim.AnimationTimer += delta;
            if (anim.AnimationTimer >= anim.RandomAnimationTime)
            {
                SetAnimationStateFrame(index, anim.CurrentAnimationFrame + 1);
            }
        }
    }
}
