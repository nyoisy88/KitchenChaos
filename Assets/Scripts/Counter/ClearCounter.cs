public class ClearCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            if (!HasKitchenObject())
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
                return;
            }

            KitchenObject playerKO = player.GetKitchenObject();
            KitchenObject thisKO = GetKitchenObject();

            // Case 1: Player is holding a plate
            if (playerKO.TryGetPlate(out PlateKitchenObject playerPlate))
            {
                if (playerPlate.TryAddIngredient(thisKO.GetKitchenObjectSO()))
                {
                    thisKO.DestroySelf();
                }
                return;
            }

            // Case 2: This counter has a plate
            if (thisKO.TryGetPlate(out PlateKitchenObject counterPlate))
            {
                if (counterPlate.TryAddIngredient(playerKO.GetKitchenObjectSO()))
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
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}
