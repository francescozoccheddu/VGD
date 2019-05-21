using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Wheeled.UI.Menu
{
    public sealed class ValidatorProxyBehaviour : ValidatorBehaviour
    {
        
        public void Validate(bool _isValid)
        {
            validated.Invoke(_isValid);
        }

    }
}
