using UnityEngine;

namespace Lightbringer
{
    public class DoubleKin : MonoBehaviour
    {
        private HealthManager _hm;
        private float         _invincibleTime = Time.deltaTime;
        private bool[]        _kinFight;
        private GameObject    _kinTwo;

        private void Start()
        {
            _hm = gameObject.GetComponent<HealthManager>();
            _kinFight = new bool[12];
        }

        private void Update()
        {
            int kinHp = _hm.hp;
            if (!_kinFight[0] && kinHp < 400)
            {
                _kinFight[0] = true;
                HeroController.instance.playerData.isInvincible = true; // temporary invincibility iFrames
                Lightbringer.SpriteFlash.flash(Color.black, 0.6f, 0.15f, 0f, 0.55f);
                _kinFight[5] = true; // iFrames
                _kinTwo = Instantiate(gameObject);
                _kinTwo.GetComponent<HealthManager>().hp = 99999;
            }
            else if (_kinFight[5]) // iFrames
            {
                _invincibleTime += Time.deltaTime;
                if (!(_invincibleTime >= 5.5f)) return;
                HeroController.instance.playerData.isInvincible = false;
                _kinFight[5] = false;
            }
            else if (!_kinFight[1] && kinHp < 1)
            {
                _kinFight[1] = true;
                _kinTwo.GetComponent<HealthManager>().hp = 1;
            }
        }
    }
}