using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class UniqueReferenceLinkSystem : UnitySystemBase {
		readonly CellService _cellService;
		readonly StorageIdService _storageIdService;
		readonly ItemIdService _itemIdService;

		readonly QueryDescription _needCreateQuery = new QueryDescription()
			.WithAll<NeedCreateUniqueReference>();

		readonly QueryDescription _uniqueReferenceIdQuery = new QueryDescription()
			.WithAll<UniqueReferenceId>();

		public UniqueReferenceLinkSystem(World world, CellService cellService, StorageIdService storageIdService, ItemIdService itemIdService) : base(world) {
			_cellService = cellService;
			_storageIdService = storageIdService;
			_itemIdService = itemIdService;
		}

		public override void Update(in SystemState _) {
			World.Query(_needCreateQuery, (Entity _, ref NeedCreateUniqueReference needCreateUniqueReference) => {
				var targetEntity = GetTargetEntity(needCreateUniqueReference.Id);
				var gameObject = needCreateUniqueReference.GameObject;
				var transform = gameObject.transform;
				var position = new Vector2(transform.position.x, transform.position.y);

				ConfigureEntity(targetEntity, gameObject, position);

				var options = needCreateUniqueReference.Options;
				if (options.HasFlag(AdditionalComponentOptions.WorldPosition)) {
					if (!targetEntity.Has<WorldPosition>()) {
						targetEntity.Add(new WorldPosition {
							Position = position
						});
					}
				}
				if (options.HasFlag(AdditionalComponentOptions.Camera)) {
					targetEntity.Add(new CameraReference {
						Camera = gameObject.GetComponent<Camera>()
					});
				}
				if (options.HasFlag(AdditionalComponentOptions.ManualMovable)) {
					targetEntity.Add(new IsManualMovable());
				}
				if (options.HasFlag(AdditionalComponentOptions.OnCell)) {
					var cellPosition = _cellService.GetCellPosition(position);
					targetEntity.Add(new OnCell {
						Position = cellPosition
					});
					if (options.HasFlag(AdditionalComponentOptions.Obstacle)) {
						targetEntity.Add(new Obstacle());
						_cellService.TryLockCell(cellPosition);
					}
				}

				if (options.HasFlag(AdditionalComponentOptions.Storage) || options.HasFlag(AdditionalComponentOptions.StorageWithTestItem)) {
					if (!targetEntity.Has<ItemStorage>()) {
						var storageId = _storageIdService.GenerateId();
						targetEntity.Add(new ItemStorage {
							StorageId = storageId
						});

						if (options.HasFlag(AdditionalComponentOptions.StorageWithTestItem)) {
							CreateTestItems(storageId);
						}
					}
				}
			});
		}

		Entity GetTargetEntity(string targetUniqueReferenceId) {
			var targetEntityId = Entity.Null;
			World.Query(_uniqueReferenceIdQuery, (Entity uniqueReferenceEntity, ref UniqueReferenceId uniqueReferenceId) => {
				if (uniqueReferenceId.Id == targetUniqueReferenceId) {
					targetEntityId = uniqueReferenceEntity;
				}
			});
			if (!World.IsAlive(targetEntityId)) {
				targetEntityId = this.World.Create();
				targetEntityId.Add(new UniqueReferenceId {
					Id = targetUniqueReferenceId
				});
			}
			return targetEntityId;
		}

		void ConfigureEntity(Entity entity, GameObject gameObject, Vector2 position) {
			entity.Add(new GameObjectReference {
				GameObject = gameObject
			});
		}

		void CreateTestItems(long storageId) {
			CreateTestItem(storageId, "TestItem1", 1);
			CreateTestItem(storageId, "TestItem2", 2);
		}

		void CreateTestItem(long storageId, string itemId, int count) {
			var itemEntity = this.World.Create();
			itemEntity.Add(new Item {
				ResourceID = itemId,
				UniqueID = _itemIdService.GenerateId(),
				Count = count
			});
			itemEntity.Add(new ItemOwner {
				StorageId = storageId
			});
		}
	}
}
