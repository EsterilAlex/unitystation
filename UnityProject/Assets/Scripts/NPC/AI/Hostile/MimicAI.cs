using NaughtyAttributes;
using UnityEngine;

namespace Systems.MobAIs {
    public class MimicAI : GenericHostileAI {
        private int _objsMask;
        [SerializeField] private Sprite _mimicSprite;
        private SpriteRenderer _mimicSpriteRenderer;
        [SerializeField] [ReadOnly] private bool _isHiden = false;
        private float _searchDistance = 15f;
        private float _attackDistance = 2.5f;

        protected override void Awake()
        {
            base.Awake();

            _objsMask = LayerMask.GetMask("Items", "Machines", "Furniture");
            _mimicSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected override void OnSpawnMob()
        {
            base.OnSpawnMob();
            ResetForm();
        }

        /// <summary>
        /// Reset Mimic form to default
        /// </summary>
        private void ResetForm()
        {
            _mimicSpriteRenderer.sprite = _mimicSprite;
            _isHiden = false;
            SetStunned(false);
        }

        /// <summary>
        /// What happens if the mob is searching
        /// </summary>
        protected override void HandleSearch()
        {
            searchWaitTime += Time.deltaTime;
            if (searchWaitTime < searchTickRate) return;
            searchWaitTime = 0f;

            var findTarget = SearchForTarget();

            if (_isHiden)
            {
                if (findTarget != null)
                {
                    if (InRange(findTarget, _attackDistance))
                    {
                        BeginAttack(findTarget);
                    }
                }
            }
            else {
                moveWaitTime += Time.deltaTime;

                if (findTarget != null) {
                    if (InRange(findTarget, _attackDistance))
                    {
                        BeginAttack(findTarget);
                    }
                    else {
                        BeginHiding();
                    }
                }
                else {
                    ResetForm();
                    BeginSearch();
                }
            }
        }

        private void BeginHiding()
        {
            if (_isHiden) {
                StopFollowing();
                mobExplore.Priority = 0;
                mobExplore.DoAction();
                return;
            }

            GameObject targetObj = SearchForObject();
            if (targetObj == null) {
                BeginSearch();
                return;
            }

            var sprite = targetObj.GetComponentInChildren<SpriteRenderer>().sprite;
            _mimicSpriteRenderer.sprite = sprite;

            Chat.AddActionMsgToChat(gameObject, "Change form",
                $"Mimic has morphed into {targetObj.name}");

            BeginSearch();
            _isHiden = true;

            SetStunned(true);
        }

        private void SetStunned(bool stunned) {
            mobExplore.IsStunned = stunned;
        }

        protected override void BeginAttack(GameObject target)
        {
            ResetForm();
            base.BeginAttack(target);
        }

        protected override void OnAttackReceived(GameObject damagedBy = null)
        {
            ResetForm();
            ResetBehaviours();

            fleeingStopped.AddListener(BeginHiding);
            StartFleeing(damagedBy, 5f);
        }

        /// <summary>
        /// Check if object in range
        /// </summary>
        private bool InRange(GameObject target, float distance)
        {
            return Vector2.Distance(registerObject.WorldPositionServer.To2Int(), target.transform.position) <= distance;
        }

        /// <summary>
        /// Looks around and tries to find objects
        /// </summary>
        /// <returns>Found Gameobject</returns>
        protected virtual GameObject SearchForObject()
        {
            var items = Physics2D.OverlapCircleAll(registerObject.WorldPositionServer.To2Int(),
                _searchDistance, _objsMask);

            if (items.Length == 0) {
                return null;
            }

            foreach (var coll in items)
            {
                if (MatrixManager.Linecast(
                    gameObject.AssumedWorldPosServer(),
                    LayerTypeSelection.Walls,
                    null,
                    coll.gameObject.AssumedWorldPosServer()).ItHit == false) {
                    return coll.gameObject;
                }
            }

            return null;
        }

        public override void LocalChatReceived(ChatEvent chatEvent)
        {
            //do nothing
        }
    }
}