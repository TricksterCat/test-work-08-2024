namespace Runtime.Data.Weapons
{
    public class WeaponState
    {
        public WeaponState(WeaponModel model)
        {
            Model = model;
        }

        public WeaponModel Model { get; }

        public float CooldownComplete { get; private set; }
        
        public void FullCooldown(float time)
        {
            CooldownComplete = time + Model.Recharge;
        }
    }
}