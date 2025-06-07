using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Assets._Project.Scripts.Gameplay;
using Assets._Project.Scripts.UI;
using Assets._Project.Scripts.Core;
using Assets._Project.Scripts.DSL;
namespace Assets._Project.Scripts.Gameplay
{






    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 2f; 
        public float turnSpeed = 180f; 

        private Queue<IEnumerator> _actionQueue = new Queue<IEnumerator>();
        private bool _isExecutingAction = false;
        public Action OnActionQueueCompleted;

        public int ActionQueueCount => _actionQueue.Count; 

        public float checkDistance = 1.1f;
        public LayerMask wallLayer;


        private int updateCount = 0;
        private bool hasAttemptedExecuteNextActionThisFrame = false; 
        private int _actionQueueInstanceId; 
        public bool IsCurrentlyExecutingAction => _isExecutingAction;

        private bool _hasKey = false;
        public bool HasKey => _hasKey;

        void Update()
        {
            if (_actionQueue != null && _actionQueueInstanceId == 0 && _actionQueue.Count > 0) _actionQueueInstanceId = _actionQueue.GetHashCode(); 
            updateCount++;
            hasAttemptedExecuteNextActionThisFrame = false; 

            if (_actionQueue.Count > 0)
            {
                if (!_isExecutingAction)
                {
                    if (!hasAttemptedExecuteNextActionThisFrame) 
                    {
                        Debug.Log($"[PlayerController Update {GetInstanceID()}] CONDICIÓN CUMPLIDA. Iniciando ExecuteNextAction. Queue count: {_actionQueue.Count}, QueueInstance: {_actionQueueInstanceId}");
                        StartCoroutine(ExecuteNextAction()); 
                        hasAttemptedExecuteNextActionThisFrame = true;
                    }
                }
                else
                {
                    
                }
            }
        }


        void OnEnable()
        {
            _actionQueue = new Queue<IEnumerator>();
            _actionQueueInstanceId = _actionQueue.GetHashCode();
            Debug.Log($"====== PlayerController ({this.gameObject.name} - ID: {GetInstanceID()}) ENABLED. ActionQueue re-initialized (ID: {_actionQueueInstanceId}) ======");
        }

        void OnDisable()
        {
            Debug.Log($"====== PlayerController ({this.gameObject.name} - ID: {GetInstanceID()}) DISABLED ======");
        }


        private IEnumerator ExecuteNextAction()
        {
            Debug.Log($"====== EXECUTE NEXT ACTION (PC ID: {GetInstanceID()}, QueueInstance: {_actionQueueInstanceId}) COROUTINE STARTED ======");

            if (_actionQueue.Count == 0)
            {
                Debug.LogWarning("[PlayerController ExecuteNextAction] Queue empty at the very start of coroutine. Exiting.");
                yield break;
            }
            _isExecutingAction = true;
            Debug.Log($"[PlayerController ExecuteNextAction {GetInstanceID()}] _isExecutingAction set to true. Queue count before WaitForSeconds: {_actionQueue.Count}");

            yield return new WaitForSeconds(0.1f);

            if (_actionQueue.Count == 0)
            {
                Debug.LogWarning($"[PlayerController ExecuteNextAction {GetInstanceID()}] Queue empty after WaitForSeconds. Exiting.");
                _isExecutingAction = false; 
                yield break;
            }

            IEnumerator action = _actionQueue.Dequeue();
            Debug.Log($"[PlayerController ExecuteNextAction {GetInstanceID()}] Dequeued action. Remaining in queue: {_actionQueue.Count}. Action is null: {(action == null)}");
            if (action == null)
            {
                Debug.LogError("[PlayerController ExecuteNextAction] La acción obtenida de la cola es NULL! Saliendo.");
                _isExecutingAction = false;
                yield break;
            }

            Debug.Log($"[PlayerController ExecuteNextAction {GetInstanceID()}] A PUNTO DE INICIAR LA SUB-CORRUTINA (action)...");
            yield return StartCoroutine(action);
            Debug.Log($"[PlayerController ExecuteNextAction {GetInstanceID()}] Corrutina de acción (action) SE HA COMPLETADO.");
            _isExecutingAction = false;
            Debug.Log($"[PlayerController ExecuteNextAction {GetInstanceID()}] _isExecutingAction = false");

            if (_actionQueue.Count == 0)
            {
                Debug.Log($"[PlayerController ExecuteNextAction {GetInstanceID()}] Cola de acciones ahora vacía. Disparando OnActionQueueCompleted.");
                OnActionQueueCompleted?.Invoke();
            }
        }


        public void QueueMove(int steps)
        {
            if (steps == 0)
            {
                Debug.Log($"[PlayerController QueueMove {GetInstanceID()}] llamado con 0 pasos. Ignorando."); // LOG
                return;
            }
            Debug.Log($"[PlayerController QueueMove {GetInstanceID()}] llamado con {steps} pasos. QueueInstance: {_actionQueueInstanceId}");
            _actionQueue.Enqueue(MoveAction(steps));
            Debug.Log($"[PlayerController QueueMove {GetInstanceID()}] MoveAction encolado. Queue count: {_actionQueue.Count}, QueueInstance: {_actionQueueInstanceId}"); // LOG
        }

        public void QueueTurn(float angle)
        {
            if (angle == 0) return;
            Debug.Log($"[PlayerController QueueTurn {GetInstanceID()}] llamado con {angle} grados. QueueInstance: {_actionQueueInstanceId}");
            _actionQueue.Enqueue(TurnAction(angle));
        }

        private IEnumerator MoveAction(int steps)
        {
            Debug.Log($"====== MOVE ACTION (PC ID: {GetInstanceID()}) ({steps} pasos) HA SIDO LLAMADA Y HA COMENZADO A EJECUTARSE! QueueInstance: {_actionQueueInstanceId} ======");
            yield return new WaitForSeconds(0.1f); 



            Debug.Log($"====== MOVE ACTION ({steps} pasos) HA SIDO LLAMADA ======"); 
            Debug.Log($"[PlayerController MoveAction {GetInstanceID()}] CORRUTINA INICIADA para {steps} pasos. moveSpeed: {moveSpeed}, Time.timeScale: {Time.timeScale}"); // LOG
            Vector3 targetDirection = transform.forward;
            float distanceToMove = (float)steps;

            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + targetDirection * distanceToMove;

            float singleStepDuration = 1f / moveSpeed;
            float totalDuration = distanceToMove * singleStepDuration;
            float elapsedTime = 0f;

            Debug.Log($"[PlayerController MoveAction {GetInstanceID()}] StartPos: {startPosition}, TargetPos: {targetPosition}, Dist: {distanceToMove}, Est.Duration: {totalDuration}"); // LOG

            if (moveSpeed <= 0)
            {
                Debug.LogError($"[PlayerController MoveAction {GetInstanceID()}] moveSpeed es 0 o negativo. El personaje no se moverá."); // LOG
                yield break; 
            }
            else
            {
                Debug.Log($"[PlayerController MoveAction {GetInstanceID()}] moveSpeed: {moveSpeed} es válido.");
            }

            while (elapsedTime < totalDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / totalDuration);
                transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                yield return null; 
            }

            transform.position = targetPosition; 
            Debug.Log($"[PlayerController MoveAction {GetInstanceID()}] CORRUTINA TERMINADA. Posición final: {transform.position}"); 
        }

        private IEnumerator TurnAction(float angle)
        {
            Debug.Log($"[PlayerController TurnAction {GetInstanceID()}] Turning {angle} degrees. QueueInstance: {_actionQueueInstanceId}");
            Quaternion startRotation = transform.rotation; 
            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, angle, 0);
            float t = 0f;
            float duration = Mathf.Abs(angle) / turnSpeed;

            while (t < duration)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t / duration);
                yield return null;
            }
            transform.rotation = targetRotation; 
        }

        public void ResetPlayer(Vector3 startPosition, Quaternion startRotation)
        {
            Debug.Log($"[PlayerController ResetPlayer {GetInstanceID()}] llamado. Limpiando acciones y reseteando estado. Current QueueInstance: {_actionQueueInstanceId}");
            ClearActionQueue();
            transform.position = startPosition;
            transform.rotation = startRotation;
            _hasKey = false; 
            Debug.Log($"[PlayerController ResetPlayer {GetInstanceID()}] Player Reset. New QueueInstance: {_actionQueueInstanceId}");
        }


        public void ClearActionQueue() 
        {
            Debug.Log($"[PlayerController ClearActionQueue {GetInstanceID()}] Limpiando cola de acciones. Current QueueInstance: {_actionQueueInstanceId}");
            _actionQueue.Clear();
            StopAllCoroutines();
            _isExecutingAction = false; 
        }


        public bool CheckForWallAhead()
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            RaycastHit hit;

            Debug.DrawRay(rayOrigin, transform.forward * checkDistance, Color.yellow, 1f);

            if (Physics.Raycast(rayOrigin, transform.forward, out hit, checkDistance, wallLayer))
            {
                Debug.Log($"[PlayerController CheckForWallAhead {GetInstanceID()}] Wall detected ahead!");
                return true;
            }
            return false;
        }




        public void TryPickUpKey()
        {
            if (_hasKey)
            {
                Debug.Log("[PlayerController] Ya tiene la llave.");
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, 0.4f);
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Key")) 
                {
                    KeyController keyCtrl = col.GetComponent<KeyController>();
                    if (keyCtrl != null && keyCtrl.IsAvailable())
                    {
                        _hasKey = true;
                        keyCtrl.Collect(); 
                        Debug.Log("[PlayerController] Llave recogida!");
                        return; 
                    }
                }
            }
            Debug.Log("[PlayerController] No hay llave para recoger aquí o ya fue recogida.");
        }




        public void TryOpenDoor()
        {
            if (!_hasKey)
            {
                Debug.Log("[PlayerController] Intento de abrir puerta sin llave.");
                return;
            }

            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            float doorCheckDistance = 1.1f; 
            Debug.DrawRay(rayOrigin, transform.forward * doorCheckDistance, Color.blue, 1f);

            if (Physics.Raycast(rayOrigin, transform.forward, out hit, doorCheckDistance))
            {
                if (hit.collider.CompareTag("Door")) 
                {
                    DoorController doorCtrl = hit.collider.GetComponent<DoorController>();
                    if (doorCtrl != null && !doorCtrl.IsOpen)
                    {
                        doorCtrl.Open(); 
                        Debug.Log("[PlayerController] Puerta abierta!");
                        return;
                    }
                    else if (doorCtrl != null && doorCtrl.IsOpen)
                    {
                        Debug.Log("[PlayerController] La puerta ya está abierta.");
                    }
                }
            }
            Debug.Log("[PlayerController] No hay puerta para abrir adelante o ya está abierta.");

        }








    }


}