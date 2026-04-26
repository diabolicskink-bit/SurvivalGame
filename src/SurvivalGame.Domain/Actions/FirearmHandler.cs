namespace SurvivalGame.Domain;

public sealed class FirearmHandler : IActionHandler
{
    public IReadOnlyList<GameActionKind> HandledKinds { get; } = new[]
    {
        GameActionKind.LoadFeedDevice,
        GameActionKind.UnloadFeedDevice,
        GameActionKind.InsertFeedDevice,
        GameActionKind.RemoveFeedDevice,
        GameActionKind.LoadWeapon,
        GameActionKind.ReloadWeapon,
        GameActionKind.TestFire,
        GameActionKind.LoadStatefulFeedDevice,
        GameActionKind.UnloadStatefulFeedDevice,
        GameActionKind.InsertStatefulFeedDevice,
        GameActionKind.RemoveStatefulFeedDevice,
        GameActionKind.LoadStatefulWeapon,
        GameActionKind.ReloadStatefulWeapon,
        GameActionKind.TestFireStatefulWeapon,
        GameActionKind.InstallStatefulWeaponMod,
        GameActionKind.RemoveStatefulWeaponMod,
        GameActionKind.ShootNpc
    };

    public IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context)
    {
        if (context.FirearmActions is null)
        {
            return Array.Empty<AvailableAction>();
        }

        return context.FirearmActions
            .GetAvailableActions(context.State)
            .Concat(context.FirearmActions.GetAvailableStatefulActions(context.State, context.ItemCatalog));
    }

    public GameActionResult Handle(GameActionRequest request, GameActionContext context)
    {
        return request switch
        {
            LoadFeedDeviceActionRequest loadFeed => ExecuteFirearmAction(
                context,
                service => service.LoadFeedDevice(context.State, loadFeed.FeedDeviceItemId, loadFeed.AmmunitionItemId)
            ),
            UnloadFeedDeviceActionRequest unloadFeed => ExecuteFirearmAction(
                context,
                service => service.UnloadFeedDevice(context.State, unloadFeed.FeedDeviceItemId)
            ),
            InsertFeedDeviceActionRequest insertFeed => ExecuteFirearmAction(
                context,
                service => service.InsertFeedDevice(context.State, insertFeed.WeaponItemId, insertFeed.FeedDeviceItemId)
            ),
            RemoveFeedDeviceActionRequest removeFeed => ExecuteFirearmAction(
                context,
                service => service.RemoveFeedDevice(context.State, removeFeed.WeaponItemId)
            ),
            LoadWeaponActionRequest loadWeapon => ExecuteFirearmAction(
                context,
                service => service.LoadWeapon(context.State, loadWeapon.WeaponItemId, loadWeapon.AmmunitionItemId)
            ),
            ReloadWeaponActionRequest reloadWeapon => ExecuteFirearmAction(
                context,
                service => service.ReloadWeapon(context.State, reloadWeapon.WeaponItemId, reloadWeapon.AmmunitionItemId)
            ),
            TestFireActionRequest testFire => ExecuteFirearmAction(
                context,
                service => service.TestFire(context.State, testFire.WeaponItemId)
            ),
            LoadStatefulFeedDeviceActionRequest loadStatefulFeed => ExecuteFirearmAction(
                context,
                service => service.LoadStatefulFeedDevice(context.State, loadStatefulFeed.FeedDeviceItemId, loadStatefulFeed.AmmunitionItemId)
            ),
            UnloadStatefulFeedDeviceActionRequest unloadStatefulFeed => ExecuteFirearmAction(
                context,
                service => service.UnloadStatefulFeedDevice(context.State, unloadStatefulFeed.FeedDeviceItemId)
            ),
            InsertStatefulFeedDeviceActionRequest insertStatefulFeed => ExecuteFirearmAction(
                context,
                service => service.InsertStatefulFeedDevice(context.State, insertStatefulFeed.WeaponItemId, insertStatefulFeed.FeedDeviceItemId)
            ),
            RemoveStatefulFeedDeviceActionRequest removeStatefulFeed => ExecuteFirearmAction(
                context,
                service => service.RemoveStatefulFeedDevice(context.State, removeStatefulFeed.WeaponItemId)
            ),
            LoadStatefulWeaponActionRequest loadStatefulWeapon => ExecuteFirearmAction(
                context,
                service => service.LoadStatefulWeapon(context.State, loadStatefulWeapon.WeaponItemId, loadStatefulWeapon.AmmunitionItemId)
            ),
            ReloadStatefulWeaponActionRequest reloadStatefulWeapon => ExecuteFirearmAction(
                context,
                service => service.ReloadStatefulWeapon(context.State, reloadStatefulWeapon.WeaponItemId, reloadStatefulWeapon.AmmunitionItemId)
            ),
            TestFireStatefulWeaponActionRequest testStatefulWeapon => ExecuteFirearmAction(
                context,
                service => service.TestFireStatefulWeapon(context.State, testStatefulWeapon.WeaponItemId)
            ),
            InstallStatefulWeaponModActionRequest installWeaponMod => ExecuteFirearmAction(
                context,
                service => service.InstallStatefulWeaponMod(context.State, installWeaponMod.WeaponItemId, installWeaponMod.ModItemId)
            ),
            RemoveStatefulWeaponModActionRequest removeWeaponMod => ExecuteFirearmAction(
                context,
                service => service.RemoveStatefulWeaponMod(context.State, removeWeaponMod.WeaponItemId, removeWeaponMod.SlotId)
            ),
            ShootNpcActionRequest shootNpc => ShootNpc(context, shootNpc.TargetNpcId),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult ExecuteFirearmAction(
        GameActionContext context,
        Func<FirearmActionService, GameActionResult> action)
    {
        if (context.FirearmActions is null)
        {
            return GameActionResult.Failure("Firearm actions are not available.");
        }

        var result = action(context.FirearmActions);
        if (!result.Succeeded || result.ElapsedTicks == 0)
        {
            if (result.Succeeded)
            {
                context.SynchronizeStatefulInventoryPlacements();
            }

            return result;
        }

        context.State.AdvanceTime(result.ElapsedTicks);
        context.SynchronizeStatefulInventoryPlacements();
        return GameActionResult.Success(
            result.ElapsedTicks,
            result.Messages.Concat(new[] { $"Time +{result.ElapsedTicks}." }).ToArray()
        );
    }

    private static GameActionResult ShootNpc(GameActionContext context, NpcId targetNpcId)
    {
        if (context.FirearmActions is null)
        {
            return GameActionResult.Failure("Firearm actions are not available.");
        }

        var result = context.FirearmActions.ShootEquippedNpc(context.State, targetNpcId);
        if (!result.Succeeded)
        {
            return result;
        }

        context.State.AdvanceTime(GameActionPipeline.ShootTickCost);
        return GameActionResult.Success(
            GameActionPipeline.ShootTickCost,
            result.Messages.Concat(new[] { $"Time +{GameActionPipeline.ShootTickCost}." }).ToArray()
        );
    }
}
