public class ClearCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        // Player is holding Kitchen Object
        if (player.HasKitchenObject())
        {
            // Counter is empty
            if (!HasKitchenObject())
            {
                player.GetKitchenObject().SetKitchenObjectParentRpc(this);
                return;
            }

            KitchenObject playerKO = player.GetKitchenObject();
            KitchenObject thisKO = GetKitchenObject();

            // Case 1: Player is holding a plate and this counter has a Kitchen Object
            if (playerKO.TryGetPlate(out PlateKitchenObject playerPlate))
            {
                if (playerPlate.TryAddIngredientRpc(thisKO.GetKitchenObjectSO()))
                {
                    thisKO.DestroySelf();
                }
                return;
            }

            // Case 2: Player is holding a Kitchen Object and This counter has a plate
            if (thisKO.TryGetPlate(out PlateKitchenObject counterPlate))
            {
                if (counterPlate.TryAddIngredientRpc(playerKO.GetKitchenObjectSO()))
                {
                    playerKO.DestroySelf();
                }
                return;
            }

        }
        else
        {
            if (HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParentRpc(player);
            }
        }
    }
}
