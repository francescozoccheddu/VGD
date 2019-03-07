using System.Collections.Generic;
using UnityEngine;

namespace Wheeled.Gameplay
{
    public sealed partial class PlayerBehaviour
    {

        private sealed class ActionHistory
        {

            public interface Action
            {
                void FullRun(PlayerBehaviour _playerBehaviour);
            }

            public struct DieAction : Action
            {
                public Vector3 hitDirection;
                public Vector3 hitPoint;
                public bool exploded;

                public void FullRun(PlayerBehaviour _playerBehaviour)
                {
                    _playerBehaviour.Die(hitDirection, hitPoint, exploded);
                }
            }

            public struct KazeAction : Action
            {
                public float intensity;

                public void FullRun(PlayerBehaviour _playerBehaviour)
                {
                }
            }

            public struct SpawnAction : Action
            {
                public int spawnPoint;

                public void FullRun(PlayerBehaviour _playerBehaviour)
                {
                    _playerBehaviour.Spawn(spawnPoint);
                }
            }

            public struct ShootAction : Action
            {

                public Vector3 direction;
                public WeaponType weapon;
                public float intensity;

                public void FullRun(PlayerBehaviour _playerBehaviour)
                {
                }
            }

            private struct Node
            {
                public Time time;
                public Action action;
            }

            private readonly LinkedList<Node> m_actions = new LinkedList<Node>();

            public void Add(Time _time, Action _action)
            {
                LinkedListNode<Node> previous = m_actions.Last;
                while (previous != null && previous.Value.time > _time)
                {
                    previous = previous.Previous;
                }
                Node node = new Node { time = _time, action = _action };
                if (previous == null)
                {
                    m_actions.AddFirst(node);
                }
                else
                {
                    m_actions.AddAfter(previous, node);
                }
            }

            public void Run(Time _time, PlayerBehaviour _playerBehaviour)
            {
                while (m_actions.First != null && m_actions.First.Value.time <= _time)
                {
                    m_actions.First.Value.action.FullRun(_playerBehaviour);
                    m_actions.RemoveFirst();
                }
            }

        }

        private sealed class LifeHistory
        {

            private readonly LinkedList<Node> m_nodes = new LinkedList<Node>();

            private struct Node
            {
                public Time time;
                public bool alive;
            }

            public void Add(Time _time, bool _alive)
            {
                LinkedListNode<Node> previous = m_nodes.Last;
                while (previous?.Value.time > _time)
                {
                    previous = previous.Previous;
                }
                Node node = new Node { time = _time, alive = _alive };
                if (previous == null)
                {
                    m_nodes.AddFirst(node);
                }
                else if (previous.Value.time == _time)
                {
                    previous.Value = node;
                }
                else
                {
                    m_nodes.AddAfter(previous, node);
                }
            }

            public bool? Get(Time _time, out float _outElapsedTime)
            {
                LinkedListNode<Node> node = m_nodes.First;
                while (node?.Next?.Value.time <= _time)
                {
                    node = node.Next;
                }
                _outElapsedTime = (_time - (node?.Value.time ?? _time)).RealTime;
                return node?.Value.alive;
            }

            public void Forget(Time _time)
            {
                while (m_nodes.First?.Next?.Value.time <= _time)
                {
                    m_nodes.RemoveFirst();
                }
            }

        }

        private readonly ActionHistory m_actionHistory = new ActionHistory();
        private readonly LifeHistory m_statusHistory = new LifeHistory();

        private void SetAlive(bool _alive)
        {
            actorRenderer.enabled = _alive;
        }

        private void Spawn(int _spawnPoint)
        {
            m_statusHistory.Add(m_validationTime, true);
            m_simulationHistory.Set(new MoveHistory.Node
            {
                simulation = new SimulationState
                {
                    // TODO Get spawn position
                    position = new Vector3(0, 2, 0)
                }
            }, m_validationTime.Node);
            // TODO Spawn effect
            Debug.Log("Spawned");
        }

        private void Spawn()
        {
            // TODO Choose spawn point
            Spawn(0);
        }

        private const float c_respawnTime = 3.0f;

        private void Die(Vector3 _hitDirection, Vector3 _hitPoint, bool _exploded)
        {
            m_statusHistory.Add(m_validationTime, false);
        }

        public void UpdateStatus()
        {
            m_actionHistory.Run(m_validationTime, this);
            bool? alive = m_statusHistory.Get(m_presentationTime, out float statusAge);
            if (alive != null)
            {
                SetAlive(alive.Value);
            }
            if (isAuthoritative && (alive == null || (alive == false && statusAge > c_respawnTime)))
            {
                Spawn();
                // TODO Tell clients
            }
            m_statusHistory.Forget(m_validationTime);
        }

    }
}