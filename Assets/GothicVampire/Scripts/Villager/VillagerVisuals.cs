using UnityEngine;

namespace GothicVampire.Villagers
{
    public enum AnimationState
    {
        Idle,
        Walking,
    }

    public class VillagerVisuals : MonoBehaviour
    {
        [Header("Look Offset")]
        [SerializeField] private Vector3 _lookOffset;
        public SpriteRenderer SpriteRenderer => _sprite;

        private Camera _mainCam;
        private Vector3 prevPos;

        private Villager _villager;
        private GameObject _curVisualModel;
        private SpriteRenderer _sprite;
        private Animator _anim;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _mainCam = Camera.main;
            prevPos = transform.position;
        }

        void LateUpdate()
        {
            transform.forward = _mainCam.transform.forward + _lookOffset;
        }

        public void Initialize(Villager villager)
        {
            _villager = villager;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            _curVisualModel = _villager.Model;
            _sprite = _curVisualModel.GetComponent<SpriteRenderer>();
            _anim = _curVisualModel.GetComponent<Animator>();
        }

        public void UpdateAnimationState(AnimationState state)
        {
            _anim.SetInteger("State", (int)state);

            var randomInt = Random.Range(0, 3);
            _anim.SetFloat("Idle", (float)randomInt);
        }

        public void UpdateSpriteFlip()
        {
            float dx = transform.position.x - prevPos.x;
            float dz = transform.position.z - prevPos.z;

            // RULE SET:
            // X higher  = Flip X
            // Z higher  = Unflip X
            // Z lower   = Flip X
            // X lower   = Unflip X

            bool flipX;

            if (Mathf.Abs(dx) >= Mathf.Abs(dz))
            {
                // Movement is mostly horizontal (X direction)
                flipX = dx > 0;    // X higher → Flip  
                                   // X lower → Unflip
            }
            else
            {
                // Movement is mostly vertical (Z direction)
                flipX = dz < 0;    // Z lower → Flip  
                                   // Z higher → Unflip
            }

            _sprite.flipX = flipX;
            prevPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }


    }
}
