using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace Wheeled.Tutorial
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class DirectorBehaviour : MonoBehaviour
    {

        public PlayableAsset[] clips;

        private int m_step;
        private PlayableDirector m_director;

        private void Start()
        {
            m_step = 0;
            m_director = GetComponent<PlayableDirector>();
            StartStep();
        }

        private void StartStep() => m_director.Play(clips[m_step], DirectorWrapMode.None);


        public void Complete(int _step, float _delay = 0.0f)
        {
            if (_delay > 0.0f)
            {
                IEnumerator CompleteDelayed()
                {
                    yield return new WaitForSeconds(_delay);
                    Complete(_step);
                }
                StartCoroutine(CompleteDelayed());
            }
            else
            {
                if (_step == m_step)
                {
                    m_step++;
                }
                StartStep();
            }
        }


    }
}
