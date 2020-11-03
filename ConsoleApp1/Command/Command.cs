namespace ConsoleApp1
{
    
    public class Command
    {
        public byte[] bytes;
        public Conn conn;

        public enum FirstCommands
        {
            LoginCommand=1, ChatCommad, SelectFriendsCommand, HeartbeatCommand,
            RegisteCommand, RankCommand, RoomCommand, GameCommand
        }

        public virtual void Init(byte[] bts, Conn cnn)
        {
            bytes = bts;
            conn = cnn;
        }

        public virtual void DoCommand() { }
    }
}
