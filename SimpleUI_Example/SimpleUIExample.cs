using GTA; // This is a reference that is needed! do not edit this
using GTA.Native; // This is a reference that is needed! do not edit this
using System; // This is a reference that is needed! do not edit this
using System.Windows.Forms; // This is a reference that is needed! do not edit this

namespace SimpleUIExample
{
    public class SimpleUIExample : Script // declare Modname as a script
    {

        public SimpleUIExample() // main function
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            Interval = 0;
        }

        void OnTick(object sender, EventArgs e) // This is where most of your script goes
        {
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
        }
    }
}