using Cysharp.Threading.Tasks;
using Runtime.Commands;
using VitalRouter;

namespace Runtime.UI.Impl.Game
{
    public class GameOverPopup : View
    {
        public void ToMenu()
        {
            Router.Default.Enqueue(new HideViewCommand(ID));
            Router.Default.Enqueue(new RequestViewCommand(ViewKey.Menu));
        }
    }
}