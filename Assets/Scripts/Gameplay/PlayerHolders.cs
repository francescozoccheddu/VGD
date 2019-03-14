using UnityEngine;
using Wheeled.Core;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Gameplay
{

    internal static class PlayerHolders
    {

        public static InteractivePlayerHolder NewInteractivePlayer()
        {
            InteractivePlayerHolder playerHolder = new InteractivePlayerHolder();
            new UpdatableHolder(playerHolder)
            {
                IsRunning = true
            };
            return playerHolder;
        }

        public static AuthoritativePlayerHolder NewAuthoritativePlayer()
        {
            AuthoritativePlayerHolder playerHolder = new AuthoritativePlayerHolder();
            new UpdatableHolder(playerHolder)
            {
                IsRunning = true
            };
            return playerHolder;
        }

        public sealed class InteractivePlayerHolder : IUpdatable
        {

            public readonly MovementController m_movementController;
            public readonly PlayerView m_view;

            public InteractivePlayerHolder()
            {
                m_movementController = new MovementController(10.0f);
                m_view = new PlayerView();
            }

            void IUpdatable.Update()
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    m_movementController.Teleport(new Snapshot(), false);
                    Debug.Log("Teleport");
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    m_movementController.StartAt(RoomTime.Now, TimeStep.zero, false);
                    Debug.Log("StartAt (Now)");
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    m_movementController.Pause(true);
                    Debug.Log("Pause");
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    m_movementController.StartAt(RoomTime.Now + new TimeStep(0, 2), TimeStep.zero, true);
                    Debug.Log("StartAt (Later)");
                }
                if (Input.GetKeyDown(KeyCode.K))
                {
                    m_movementController.StartAt(RoomTime.Now - new TimeStep(0, 2), TimeStep.zero, true);
                    Debug.Log("StartAt (Sooner)");
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    m_movementController.FlushRate++;
                    Debug.LogFormat("FlushRate={0}", m_movementController.FlushRate);
                }
                if (Input.GetKeyDown(KeyCode.O))
                {
                    m_movementController.FlushRate--;
                    Debug.LogFormat("FlushRate={0}", m_movementController.FlushRate);
                }
                m_movementController.Update();
                m_view.Move(m_movementController.ViewSnapshot);
            }

        }

        public sealed class AuthoritativePlayerHolder : IUpdatable, MovementValidator.IValidationTarget, MovementValidator.ICorrectionTarget
        {

            public readonly MovementValidator movementValidator;
            public readonly MovementHistory movementHistory;
            public readonly PlayerView view;

            public AuthoritativePlayerHolder()
            {
                movementValidator = new MovementValidator(10.0f)
                {
                    validationTarget = this,
                    correctionTarget = this
                };
                movementHistory = new MovementHistory();
                view = new PlayerView();
            }

            void MovementValidator.ICorrectionTarget.Corrected(int _step, in SimulationStepInfo _simulation)
            {
                Debug.LogFormat("Corrected {0}", _step);
            }

            void MovementValidator.ICorrectionTarget.Rejected(int _step, bool _newer)
            {
                Debug.LogFormat("Rejected {0} (newer={1}, currentStep={2})", _step, _newer, RoomTime.Now.Step);
            }

            void IUpdatable.Update()
            {
                movementValidator.Update();
                movementHistory.TrimOlder(RoomTime.Now.Step - 100, true);
                movementHistory.Get(RoomTime.Now, out SimulationStep? simulation, out Sight? sight);
                if (simulation != null)
                {
                    view.Move(new Snapshot { simulation = simulation.Value });
                }
            }

            void MovementValidator.IValidationTarget.Validated(int _step, in InputStep _input, in SimulationStep _simulation)
            {
                movementHistory.Put(_step, new SimulationStepInfo { input = _input, simulation = _simulation });
            }
        }

    }

}

