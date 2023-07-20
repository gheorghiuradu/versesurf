using System;

namespace Assets.Scripts.Reusable
{
    public class SecondAction
    {
        public int Second { get; set; }
        public Action Action { get; set; }
        public bool Executed { get; set; }
    }
}