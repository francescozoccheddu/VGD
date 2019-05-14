namespace Wheeled.Menu
{
    public sealed class JoinScreenValidator : ValidatorBehaviour
    {
        #region Private Fields

        private bool m_isPortValid;
        private bool m_isIPValid;

        #endregion Private Fields

        #region Public Methods

        public bool IsValid()
        {
            return m_isPortValid && m_isIPValid;
        }

        public void SetValidIP(bool _valid)
        {
            m_isIPValid = _valid;
            validated.Invoke(IsValid());
        }

        public void SetValidPort(bool _valid)
        {
            m_isPortValid = _valid;
            validated.Invoke(IsValid());
        }

        #endregion Public Methods
    }
}