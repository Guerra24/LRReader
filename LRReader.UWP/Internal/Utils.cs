﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

using AnimationContext = Microsoft.UI.Xaml.Controls.AnimationContext;
using ElementAnimator = Microsoft.UI.Xaml.Controls.ElementAnimator;

namespace LRReader.Internal
{
	// TODO: port to native.
#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
	public partial class DefaultElementAnimator : ElementAnimator
	{
		private const double DefaultAnimationDurationInMs = 150.0;
		public static double AnimationSlowdownFactor { get; set; }

		static DefaultElementAnimator()
		{
			AnimationSlowdownFactor = 1.0;
		}

		protected override bool HasShowAnimationCore(UIElement element, AnimationContext context)
		{
			return true;
		}

		protected override bool HasHideAnimationCore(UIElement element, AnimationContext context)
		{
			return true;
		}

		protected override bool HasBoundsChangeAnimationCore(
			UIElement element,
			AnimationContext context,
			Rect oldBounds,
			Rect newBounds)
		{
			return true;
		}

		protected override void StartShowAnimation(UIElement element, AnimationContext context)
		{
			var visual = ElementCompositionPreview.GetElementVisual(element);
			var compositor = visual.Compositor;

			var fadeInAnimation = compositor.CreateScalarKeyFrameAnimation();
			fadeInAnimation.InsertKeyFrame(0.0f, 0.0f);

			if (HasBoundsChangeAnimationsPending && HasHideAnimationsPending)
			{
				fadeInAnimation.InsertKeyFrame(0.66f, 0.0f);
			}
			else if (HasBoundsChangeAnimationsPending || HasHideAnimationsPending)
			{
				fadeInAnimation.InsertKeyFrame(0.5f, 0.0f);
			}

			fadeInAnimation.InsertKeyFrame(1.0f, 1.0f);
			fadeInAnimation.Duration = TimeSpan.FromMilliseconds(
				DefaultAnimationDurationInMs * ((HasHideAnimationsPending ? 1 : 0) + (HasBoundsChangeAnimationsPending ? 1 : 0) + 1) * AnimationSlowdownFactor);

			var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
			visual.StartAnimation("Opacity", fadeInAnimation);
			batch.End();
			batch.Completed += delegate { OnShowAnimationCompleted(element); };
		}

		protected override void StartHideAnimation(UIElement element, AnimationContext context)
		{
			var visual = ElementCompositionPreview.GetElementVisual(element);
			var compositor = visual.Compositor;

			var fadeOutAnimation = compositor.CreateScalarKeyFrameAnimation();
			fadeOutAnimation.InsertExpressionKeyFrame(0.0f, "this.CurrentValue");
			fadeOutAnimation.InsertKeyFrame(1.0f, 0.0f);
			fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(DefaultAnimationDurationInMs * AnimationSlowdownFactor);

			var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
			visual.StartAnimation("Opacity", fadeOutAnimation);
			batch.End();
			batch.Completed += delegate
			{
				visual.Opacity = 1.0f;
				OnHideAnimationCompleted(element); // System.AccessViolationException
			};
		}

		protected override void StartBoundsChangeAnimation(UIElement element, AnimationContext context, Rect oldBounds, Rect newBounds)
		{
			var visual = ElementCompositionPreview.GetElementVisual(element);
			var compositor = visual.Compositor;
			var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

			// Animate offset.
			if (oldBounds.X != newBounds.X ||
				oldBounds.Y != newBounds.Y)
			{
				AnimateOffset(element, visual, compositor, oldBounds, newBounds);
			}

			batch.End();
			batch.Completed += delegate { OnBoundsChangeAnimationCompleted(element); };
		}

		private void AnimateOffset(UIElement element, Visual visual, Compositor compositor, Rect oldBounds, Rect newBounds)
		{
			var offsetAnimation = compositor.CreateVector2KeyFrameAnimation();

			offsetAnimation.SetVector2Parameter("start", new Vector2(
				(float)(oldBounds.X - newBounds.X),
				(float)(oldBounds.Y - newBounds.Y)));
			offsetAnimation.InsertExpressionKeyFrame(0.0f, "start");
			if (HasHideAnimationsPending)
			{
				//offsetAnimation.InsertExpressionKeyFrame(0.5f, "delta");
			}
			offsetAnimation.SetVector2Parameter("final", new Vector2());
			offsetAnimation.InsertExpressionKeyFrame(1.0f, "final");

			offsetAnimation.Duration = TimeSpan.FromMilliseconds(
				DefaultAnimationDurationInMs * ((HasHideAnimationsPending ? 1 : 0) + 1) * AnimationSlowdownFactor);

			visual.StartAnimation("TransformMatrix._41_42", offsetAnimation);
		}
	}
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
}
