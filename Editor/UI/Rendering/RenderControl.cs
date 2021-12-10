using Avalonia.Rendering.SceneGraph;
using Avalonia.Threading;
using Avalonia.Platform;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Skia;
using SkiaSharp;
using Avalonia;

namespace BladeEngine.Editor.UI.Rendering;

public unsafe class RenderControl : UserControl
{
	private SKBitmap _renderTarget;
	private readonly DrawOp _drawOperation;

	public int RenderWidth => (int) Bounds.Width;
	public int RenderHeight => (int) Bounds.Height;
	public IntPtr RenderBuffer { get; private set; }

	public Span<byte> RenderBufferSpan => new((void*) RenderBuffer, _renderTarget.Width * _renderTarget.Height * _renderTarget.BytesPerPixel);

	public RenderControl()
	{
		ClipToBounds = true;
		_drawOperation = new DrawOp(this);
		_renderTarget = new SKBitmap(16, 16, SKColorType.Rgba8888, SKAlphaType.Opaque);
		SetRenderTargetSize();
	}

	~RenderControl()
	{
		NativeMemory.Free((void*) RenderBuffer);
	}

	public override void Render(DrawingContext context)
	{
		var bounds = _drawOperation.Bounds;
		if (bounds != Bounds) SetRenderTargetSize();
		
		OnRender();
		Graphics.ReadPixels(RenderBufferSpan);
		
		_renderTarget.SetPixels(RenderBuffer);
		context.Custom(_drawOperation);
		Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
	}

	private void SetRenderTargetSize()
	{
		NativeMemory.Free((void*) RenderBuffer);
		RenderBuffer = (IntPtr) NativeMemory.Alloc((nuint) (RenderWidth * RenderHeight * _renderTarget.BytesPerPixel));
		_renderTarget.Dispose();
		_renderTarget = new SKBitmap(RenderWidth, RenderHeight, SKColorType.Rgba8888, SKAlphaType.Opaque);
		_renderTarget.SetPixels(RenderBuffer);
		_drawOperation.Bounds = Bounds;
		OnViewResized();
	}

	public virtual void OnRender() {}
	public virtual void OnViewResized() {}

	private class DrawOp : ICustomDrawOperation
	{
		public Rect Bounds { get; set; }
		private readonly RenderControl _control;

		public DrawOp(RenderControl control)
		{
			_control = control;
		}
		
		public void Render(IDrawingContextImpl context)
		{
			var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
			if(canvas is null) return;
			
			canvas.Clear();
			canvas.DrawBitmap(_control._renderTarget, 0, 0);
		}

		public void Dispose() {}
		public bool HitTest(Point p) => false;
		public bool Equals(ICustomDrawOperation? other) => false;
	}
}