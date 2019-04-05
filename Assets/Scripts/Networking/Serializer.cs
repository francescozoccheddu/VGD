using LiteNetLib.Utils;

using System;
using System.Collections.Generic;

using UnityEngine;

using Wheeled.Gameplay;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;

namespace Wheeled.Networking
{
    internal static class Serializer
    {
        public static readonly NetDataWriter writer = new NetDataWriter(true, 128);

        #region Primitive put extension methods

        private static void Put(this NetDataWriter _netDataWriter, Enum _value)
        {
            _netDataWriter.Put(Convert.ToByte(_value));
        }

        private static void Put(this NetDataWriter _netDataWriter, in Vector3 _value)
        {
            _netDataWriter.Put(_value.x);
            _netDataWriter.Put(_value.y);
            _netDataWriter.Put(_value.z);
        }

        private static void Put(this NetDataWriter _netDataWriter, in InputStep _value)
        {
            _netDataWriter.Put(_value.dash);
            _netDataWriter.Put(_value.jump);
            _netDataWriter.Put(_value.movementX);
            _netDataWriter.Put(_value.movementZ);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SimulationStep _value)
        {
            _netDataWriter.Put(_value.velocity);
            _netDataWriter.Put(_value.position);
        }

        private static void Put(this NetDataWriter _netDataWriter, in Sight _value)
        {
            _netDataWriter.Put(_value.Turn);
            _netDataWriter.Put(_value.LookUp);
        }

        private static void Put(this NetDataWriter _netDataWriter, in Snapshot _value)
        {
            _netDataWriter.Put(_value.simulation);
            _netDataWriter.Put(_value.sight);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SimulationStepInfo _value)
        {
            _netDataWriter.Put(_value.input);
            _netDataWriter.Put(_value.simulation);
        }

        private static void Put(this NetDataWriter _netDataWriter, in PlayerInfo _value)
        {
            _netDataWriter.Put(_value.name);
        }

        private static void Put(this NetDataWriter _netDataWriter, in PlayerRecapInfo _value)
        {
            _netDataWriter.Put(_value.id);
            _netDataWriter.Put((byte) _value.kills);
            _netDataWriter.Put((byte) _value.deaths);
            _netDataWriter.Put(_value.health);
            _netDataWriter.Put(_value.ping);
        }

        private static void Put(this NetDataWriter _netDataWriter, in DamageInfo _value)
        {
            _netDataWriter.Put(_value.offensePosition);
            _netDataWriter.Put(_value.offenseType);
        }

        private static void Put(this NetDataWriter _netDataWriter, in HitConfirmInfo _value)
        {
            _netDataWriter.Put(_value.offenseType);
        }

        private static void Put(this NetDataWriter _netDataWriter, in ShotInfo _value)
        {
            _netDataWriter.Put(_value.position);
            _netDataWriter.Put(_value.sight);
            _netDataWriter.Put(_value.isRocket);
        }

        private static void Put(this NetDataWriter _netDataWriter, in DeathInfo _value)
        {
            _netDataWriter.Put(_value.killerId);
            _netDataWriter.Put(_value.offenseType);
            _netDataWriter.Put(_value.isExploded);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SpawnInfo _value)
        {
            _netDataWriter.Put(_value.spawnPoint);
        }

        private static void Put<T>(this NetDataWriter _netDataWriter, IEnumerable<T> _value, Action<NetDataWriter, T> _put)
        {
            int countPosition = _netDataWriter.Length;
            _netDataWriter.Put((byte) 0);
            int count = 0;
            foreach (T item in _value)
            {
                count++;
                _put(_netDataWriter, item);
                if (count >= 255)
                {
                    break;
                }
            }
            writer.Data[countPosition] = (byte) count;
        }

        #endregion Primitive put extension methods

        #region Movement messages

        public static void WriteMovementAndInputReplication(byte _id, int _step, IEnumerable<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementReplication);
            writer.Put(_id);
            writer.Put(_step);
            writer.Put(_snapshot);
            writer.Put(_inputSteps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteMovementNotify(int _firstStep, IEnumerable<InputStep> _steps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(Message.MovementNotify);
            writer.Put(_firstStep);
            writer.Put(_snapshot);
            writer.Put(_steps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteSimulationCorrection(int _step, in SimulationStepInfo _simulationStepInfo)
        {
            writer.Reset();
            writer.Put(Message.SimulationOrder);
            writer.Put(_step);
            writer.Put(_simulationStepInfo);
        }

        #endregion Movement messages

        #region Action messages

        public static void WriteDamageOrder(double _time, DamageInfo _info, byte _health)
        {
            writer.Reset();
            writer.Put(Message.DamageOrder);
            writer.Put(_time);
            writer.Put(_info);
            writer.Put(_health);
        }

        public static void WriteDeathOrderOrReplication(double _time, byte _id, DeathInfo _info, byte _deaths)
        {
            writer.Reset();
            writer.Put(Message.DeathOrderOrReplication);
            writer.Put(_time);
            writer.Put(_id);
            writer.Put(_info);
            writer.Put(_deaths);
        }

        public static void WriteHitConfirmOrder(double _time, HitConfirmInfo _info)
        {
            writer.Reset();
            writer.Put(Message.HitConfirmOrder);
            writer.Put(_time);
            writer.Put(_info);
        }

        public static void WriteKazeNotify(double _time)
        {
            writer.Reset();
            writer.Put(Message.KazeNotify);
            writer.Put(_time);
        }

        public static void WriteShootNotify(double _time, ShotInfo _info)
        {
            writer.Reset();
            writer.Put(Message.ShootNotify);
            writer.Put(_time);
            writer.Put(_info);
        }

        public static void WriteShootReplication(double _time, byte _id, ShotInfo _info)
        {
            writer.Reset();
            writer.Put(Message.ShootNotify);
            writer.Put(_time);
            writer.Put(_id);
            writer.Put(_info);
        }

        public static void WriteSpawnOrderOrReplication(double _time, byte _id, SpawnInfo _info)
        {
            writer.Reset();
            writer.Put(Message.SpawnOrderOrReplication);
            writer.Put(_time);
            writer.Put(_id);
            writer.Put(_info);
        }

        #endregion Action messages

        #region Room messages

        private static readonly byte[] s_timeChecksumBuffer = new byte[sizeof(double)];

        public static void WritePlayerIntroductionSync(byte _id, PlayerInfo _info)
        {
            writer.Reset();
            writer.Put(Message.PlayerIntroductionSync);
            writer.Put(_id);
            writer.Put(_info);
        }

        public static void WritePlayerWelcomeSync(byte _id)
        {
            writer.Reset();
            writer.Put(Message.PlayerWelcomeSync);
            writer.Put(_id);
            // Checksum
            writer.Put((byte) (255 - _id));
            writer.Put(_id);
            writer.Put((byte) (255 - _id));
        }

        public static void WriteQuitReplication(double _time, byte _id)
        {
            writer.Reset();
            writer.Put(Message.QuitReplication);
            writer.Put(_time);
            writer.Put(_id);
        }

        public static void WriteReady()
        {
            writer.Reset();
            writer.Put(Message.ReadyNotify);
        }

        public static void WriteRecapSync(double _time, IEnumerable<PlayerRecapInfo> _recaps)
        {
            writer.Reset();
            writer.Put(Message.RecapSync);
            writer.Put(_time);
            writer.Put(_recaps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteTimeSync(double _time)
        {
            writer.Reset();
            writer.Put(Message.TimeSync);
            writer.Put(_time);
            // Checksum
            ulong bytes = Convert.ToUInt64(_time);
            writer.Put(~bytes);
        }

        #endregion Room messages
    }
}