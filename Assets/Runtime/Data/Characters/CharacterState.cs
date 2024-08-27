namespace Runtime.Data.Characters
{
    public class CharacterState
    {
        public CharacterState(CharacterConfig config)
        {
            Config = config;
            Hp = config.Hp;
        }
        
        public CharacterConfig Config { get; }
        public float Hp { get; set; }
    }
}