namespace BladeEngine.Editor.UI.Rendering;

public class RenderView : RenderControl
{
	public override void OnRender()
	{
		Graphics.Clear();
		Graphics.FlushRenderBuffer();
		Graphics.SwapBuffers();
	}

	public override void OnViewResized()
	{
		Graphics.SetRenderingResolution((uint) RenderWidth, (uint) RenderHeight);
	}
}