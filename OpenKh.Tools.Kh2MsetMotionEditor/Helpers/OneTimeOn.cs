namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class OneTimeOn
    {
        private bool _state;

        public OneTimeOn(bool initial)
        {
            _state = initial;
        }

        public void TurnOn()
        {
            _state = true;
        }

        public bool Consume()
        {
            if (_state)
            {
                _state = false;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
