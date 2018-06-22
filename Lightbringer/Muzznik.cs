using UnityEngine;

namespace Lightbringer
{
    public class Muzznik : MonoBehaviour
    {
        private bool[] _fight;

        private HealthManager _hm;

        private GameObject _minion;

        private GameObject[] _minions;

        private void Start()
        {
            gameObject.GetComponent<tk2dSprite>().GetCurrentSpriteDef().material.mainTexture =
                Lightbringer.Instance.Sprites["Muzznik"].texture;
            _hm = gameObject.GetComponent<HealthManager>();
            _hm.hp = 1500;
            _fight = new bool[12];
            _minions = new GameObject[16];
            _minion = GameObject.Find("Fly");
            _minion.transform.SetScaleY(-1f);
            _minion.GetComponent<HealthManager>().hp = 99999;
        }


        private void Update()
        {
            int gruzHp = _hm.hp;
            if (!_fight[0] && gruzHp < 1470)
            {
                _fight[0] = true;
                _minions[0] = _minion.Spawn(gameObject.transform.position); // dud
                _minions[1] = _minion.Spawn(gameObject.transform.position);
                _minions[1].transform.SetScaleX(1.3f);
                _minions[1].transform.SetScaleY(-1.3f);
            }
            else if (!_fight[1] && gruzHp < 1100)
            {
                _fight[1] = true;
                _minions[2] = _minion.Spawn(gameObject.transform.position);
            }
            else if (!_fight[2] && gruzHp < 800)
            {
                _fight[2] = true;
                _minions[3] = _minion.Spawn(gameObject.transform.position);
                _minions[3].transform.SetScaleX(.8f);
                _minions[3].transform.SetScaleY(-.8f);
            }
            else if (!_fight[3] && gruzHp < 600)
            {
                _fight[3] = true;
                _minions[4] = _minion.Spawn(gameObject.transform.position);
                _minions[4].transform.SetScaleX(.8f);
                _minions[4].transform.SetScaleY(-.8f);
            }
            else if (!_fight[4] && gruzHp < 500)
            {
                _fight[4] = true;
                _minions[5] = _minion.Spawn(gameObject.transform.position);
            }
            else if (!_fight[5] && gruzHp < 400)
            {
                _fight[5] = true;
                _minions[6] = _minion.Spawn(gameObject.transform.position);
            }
            else if (!_fight[6] && gruzHp < 300)
            {
                _fight[6] = true;
                _minions[7] = _minion.Spawn(gameObject.transform.position);
            }
            else if (!_fight[7] && gruzHp < 200)
            {
                _fight[7] = true;
                _minions[8] = _minion.Spawn(gameObject.transform.position);
                _minions[9] = _minion.Spawn(gameObject.transform.position);
            }
            else if (!_fight[8] && gruzHp < 100)
            {
                _fight[8] = true;
                _minions[10] = _minion.Spawn(gameObject.transform.position);
                _minions[11] = _minion.Spawn(gameObject.transform.position);
            }
            else if (!_fight[9] && gruzHp < 1)
            {
                _fight[9] = true;
                _minion.GetComponent<HealthManager>().hp = 1;
                for (int i = 0; i < 12; i++)
                {
                    _minions[i].GetComponent<HealthManager>().hp = 1;
                }

            }
        }

        private void OnDestroy()
        {
            foreach (GameObject go in _minions)
            {
                Destroy(go);
            }
            Destroy(_minion);
        }
    }
}
