using Arch.Core;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class FlipSpriteMovementSystem : UnitySystemBase {
		QueryDescription _movingSpriteQuery = new QueryDescription()
			.WithAll<FlipSprite, SpriteRendererReference, MoveToPosition>();

		public FlipSpriteMovementSystem(World world) : base(world) {
		}

		public override void Update(in SystemState _) {
			World.Query(_movingSpriteQuery, (Entity entity, ref FlipSprite flipSprite, ref SpriteRendererReference spriteRendererRef, ref MoveToPosition moveToPosition) => {
				var shouldFlip = moveToPosition.NewPosition.x > moveToPosition.OldPosition.x;
				if (shouldFlip == flipSprite.IsFlipped) {
					return;
				}
				flipSprite.IsFlipped = shouldFlip;
				var spriteRenderer = spriteRendererRef.SpriteRenderer;
				if (spriteRenderer.flipX == flipSprite.IsFlipped) {
					return;
				}
				spriteRenderer.flipX = flipSprite.IsFlipped;
			});
		}
	}
}