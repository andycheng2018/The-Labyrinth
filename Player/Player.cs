using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Components;
using TMPro;
using System;
using System.Collections;
using Unity.Collections;
using Steamworks;

namespace AC
{
    public class Player : NetworkBehaviour
    {
        [Header("Player References")]
        public Camera playerCam;
        public GameObject playerObject;
        public GameObject playerUI;
        public GameObject settingsObject;
        public GameObject damageScreen;
        public GameObject winScreen;
        public TMP_Text descriptionText;
        public GameObject runestoneParticles;
        public Animator levelChange;
        public Animator levelChangeText;
        public GameObject torch;
        public ParticleSystem potionParticle;
        public Mesh[] normalMeshMale;
        public Mesh[] armMeshMale;
        public GameObject[] spawnPrefs;
        public String[] levelNames;

        [Header("Player Settings")]
        [Range(0, 100)] public float walkSpeed;
        [Range(0, 100)] public float runSpeed;
        [Range(0, 100)] public float crouchSpeed;
        [Range(0, 100)] public float jumpStrength;
        [Range(0, 100)] public float sensitivity;

        [Header("Timer Settings")]
        public TMP_Text timeText;
        public float timeRemaining;

        public NetworkVariable<float> health = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<int> audioClipNum = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        //public NetworkVariable<int> skinIndex = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkAnimator networkAnimator;
        private SettingsMenu settings;
        private Rigidbody playerRb;
        private AudioSync audioSync;
        private GameObject currentSpawnParticle;

        private Vector3 spawnPoint;
        private Vector2 velocity;
        private Vector2 frameVelocity;
        private float currentMovingSpeed;
        private int levelNumber;

        private bool isRunning;
        private bool isCrouching;
        private bool isGrounded;
        private bool isLoaded;
        private bool isOnIce;
        private bool canJump;
        private bool canAttack;
        public bool isInvincible;
        private bool timeIsRunning;

        private void Start()
        {
            networkAnimator = GetComponent<NetworkAnimator>();
            settings = settingsObject.GetComponentInParent<SettingsMenu>();
            playerRb = GetComponent<Rigidbody>();
            audioSync = GetComponent<AudioSync>();

            isGrounded = true;
            canJump = true;
            canAttack = true;
            timeIsRunning = true;

            spawnPoint = new Vector3(UnityEngine.Random.Range(-6, -10), 0, UnityEngine.Random.Range(6, 10));
            health.Value = 100;

            ChangeSkin();
            StartCoroutine(ChangePlayerPos(0, spawnPoint));
            StartCoroutine(ChangeLevel(0));
        }

        private void Update()
        {
            CheckScene();

            if (IsOwner) { 
                RotateCamera();
                GroundDetection();
                PlayerInputs();
                MovePlayer();
                DetectRaycast();
                Timer();
            }
        }

        private void CheckScene() {
            if (SceneManager.GetActiveScene().name == "(1) Main Menu")
            {
                playerCam.gameObject.SetActive(false);
                playerObject.gameObject.SetActive(false);
                playerUI.SetActive(false);
                playerRb.isKinematic = true;
                UpdateCursor(true);
            }

            if ((SceneManager.GetActiveScene().name == "(2) Game Scene" ||  SceneManager.GetActiveScene().name == "(3) Tutorial" || SceneManager.GetActiveScene().name == "(4) Testing Scene") && !isLoaded)
            {
                if (IsOwner)
                {
                    playerCam.gameObject.SetActive(true);
                    playerObject.gameObject.SetActive(true);
                    playerUI.SetActive(true);
                    playerRb.isKinematic = false;
                    UpdateCursor(false);
                    isLoaded = true;
                }
                else
                {
                    playerCam.gameObject.SetActive(false);
                    playerObject.gameObject.SetActive(true);
                    playerUI.SetActive(false);
                    playerRb.isKinematic = false;
                    playerCam.enabled = false;
                    isLoaded = true;
                }
            }
        }

        public void UpdateCursor(bool active) {
            if (active) {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else 
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void RotateCamera() {
            Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity / 40);
            frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / 1.5f);
            velocity += frameVelocity;
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);

            if (!settingsObject.activeSelf) {
                transform.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
                playerCam.transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
            }
        }

        private void GroundDetection() {
            isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.001f, Vector3.down, 0.3f);

            if (isGrounded && Physics.Raycast(transform.position + Vector3.up * 0.001f, Vector3.down, out RaycastHit other, 0.3f)) {
                if (other.transform.tag == "Wood") {
                    ChangeClipServerRpc(1);
                } else if (other.transform.tag == "Water") {
                    ChangeClipServerRpc(2);
                } else if (other.transform.tag == "Ice") {
                    ChangeClipServerRpc(3);
                } else {
                    ChangeClipServerRpc(0);
                }

                if (other.transform.tag == "Ice") {
                    isOnIce = true;
                } else {
                    isOnIce = false;
                }
            }
        }

        private void PlayerInputs() {
            isCrouching = Input.GetKey(settings.keys["Crouch"]) && isGrounded;
            isRunning = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && Input.GetKey(settings.keys["Sprint"]);

            //Animations
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                if (isCrouching)
                {
                    audioSync.ChangePitch(1.2f);
                    networkAnimator.SetTrigger("CrouchWalk");
                    currentMovingSpeed = crouchSpeed;
                }
                else if (isRunning)
                {
                    audioSync.ChangePitch(1.6f);
                    networkAnimator.SetTrigger("Run");
                    currentMovingSpeed = runSpeed;
                }
                else
                {
                    audioSync.ChangePitch(1.4f);
                    networkAnimator.SetTrigger("Walk");
                    currentMovingSpeed = walkSpeed;
                }
            }
            else
            {
                if (isCrouching && !isRunning)
                {
                    networkAnimator.SetTrigger("CrouchIdle");
                }
                else
                {
                    networkAnimator.SetTrigger("Idle");
                }
            }

            //Crouch
            if (Input.GetKeyDown(settings.keys["Crouch"]))
            {
                gameObject.GetComponent<Collider>().transform.localScale = new Vector3(1, 0.75f, 1);
            }
            if (Input.GetKeyUp(settings.keys["Crouch"]))
            {
                gameObject.GetComponent<Collider>().transform.localScale = new Vector3(1, 1, 1);
            }

            //Jump
            if (Input.GetKeyDown(settings.keys["Jump"]) && isGrounded && canJump)
            {
                playerRb.AddForce(Vector3.up * 5 * jumpStrength);
                networkAnimator.SetTrigger("Jump");
                StartCoroutine(CanJump(1f));
            }

            //Settings
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!settingsObject.activeSelf)
                {
                    settingsObject.SetActive(true);
                    UpdateCursor(true);
                }
                else
                {
                    settingsObject.SetActive(false);
                    UpdateCursor(false);
                }
            }

            //Reload
            if (Input.GetKeyDown(settings.keys["Reload"]))
            {
                levelChange.Play("LevelChange");
                StartCoroutine(ChangePlayerPos(0.5f, spawnPoint));
                StartCoroutine(ChangeLevel(0.5f));
            }

            //Health
            if (health.Value <= 0)
            {
                Respawn();               
            }

            //Attack
            if (Input.GetMouseButtonDown(0) && canAttack && Cursor.lockState != CursorLockMode.Confined) {
                networkAnimator.SetTrigger("Attack");
                StartCoroutine(CanAttack(1f));
            }
        }

        private void MovePlayer() {
            Vector2 targetVelocity = new Vector2(Input.GetAxis("Horizontal") * currentMovingSpeed / 10, Input.GetAxis("Vertical") * currentMovingSpeed / 10);
            if (isOnIce)
                playerRb.AddForce(transform.rotation * new Vector3(targetVelocity.x, playerRb.velocity.y, targetVelocity.y));
            else
                playerRb.velocity = transform.rotation * new Vector3(targetVelocity.x, playerRb.velocity.y, targetVelocity.y);
        }

        private void DetectRaycast()
        {
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit other, 10f))
            {
                if (Input.GetKeyDown(settings.keys["Interact"]))
                {
                    //Interact with object
                    if (other.transform.tag == "Lever")
                    {
                        Collider[] colliders = Physics.OverlapSphere(other.transform.position, 4.5f);
                        foreach (Collider collider in colliders)
                        {
                            if (collider.CompareTag("Gate"))
                            {
                                collider.GetComponent<Gate>().ChangeStateServerRpc();
                                collider.GetComponent<AudioSync>().PlaySound(0);
                            }
                        }
                    }
                    else if (other.transform.tag == "Chest")
                    {
                        other.transform.GetComponentInParent<Animator>().SetTrigger("Open");
                        other.transform.GetComponentInParent<AudioSource>().Play();
                        winScreen.SetActive(true);
                        UpdateCursor(true);
                        timeIsRunning = false;
                    }
                    else if (other.transform.tag == "Runestone")
                    {
                        spawnPoint = other.transform.position + new Vector3(UnityEngine.Random.Range(-2, 2), 0, UnityEngine.Random.Range(-2, 2));
                        if (currentSpawnParticle != null)
                        {
                            Destroy(currentSpawnParticle);
                        }
                        currentSpawnParticle = Instantiate(runestoneParticles, other.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
                        audioSync.PlaySound(6);
                    }
                    else if (other.transform.tag == "Torch")
                    {
                        var fire = other.transform.GetChild(0).transform.position;
                        if (IsPositionOccupied(fire, "Fire")) return;
                        SpawnObjectServerRpc(fire, Quaternion.identity, 0);
                        audioSync.PlaySound(5);
                    }
                    else if (other.transform.tag == "Coins")
                    {
                        Destroy(other.transform.gameObject);
                        settings.AddCoins(5);
                    }
                    else if (other.transform.tag == "Door")
                    {
                       other.transform.GetComponent<Door>().ChangeStateServerRpc();
                    }
                    else if (other.transform.tag == "Entrance")
                    {
                       levelChange.Play("LevelChange");
                       StartCoroutine(ChangePlayerPos(0.5f, other.transform.position - new Vector3(0, 11, 0)));
                       StartCoroutine(ChangeLevel(0.5f));
                       levelNumber++;
                    }
                    else if (other.transform.tag == "Potion") {
                        StartCoroutine(DrinkPotion(other.transform.GetComponent<Potion>()));
                        other.transform.GetComponent<NetworkObject>().Despawn(true);
                    }
                }
                else
                {
                    //Look at object
                    if (other.transform.tag == "Lever")
                    {
                        ChangeDescriptionText("Press " + settings.keys["Interact"] + " to interact");
                    }
                    else if (other.transform.tag == "Chest")
                    {
                        ChangeDescriptionText("Press " + settings.keys["Interact"] + " to win");
                    }
                    else if (other.transform.tag == "Runestone")
                    {
                        ChangeDescriptionText("Press " + settings.keys["Interact"] + " to set spawn");
                    }
                    else if (other.transform.tag == "Torch")
                    {
                        ChangeDescriptionText("Press " + settings.keys["Interact"] + " to light");
                    }
                    else if (other.transform.tag == "Statue")
                    {
                        TakeDamage(1f);
                        ChangeDescriptionText("Look away!");
                    }
                    else if (other.transform.tag == "Coins")
                    {
                        ChangeDescriptionText("Press " + settings.keys["Interact"] + " to collect");
                    }
                    else if (other.transform.tag == "Door")
                    {
                        ChangeDescriptionText("Press " + settings.keys["Interact"] + " to interact");
                    }
                    else if (other.transform.tag == "Entrance")
                    {
                        ChangeDescriptionText("Press " + settings.keys["Interact"] + " to enter the next floor");
                    }
                    else if (other.transform.tag == "Potion") {
                        ChangeDescriptionText("Press " + settings.keys["Interact"] + " to drink " + other.transform.GetComponent<Potion>().potionType);
                    }
                    else
                    {
                        descriptionText.gameObject.SetActive(false);
                    }
                }
            } 
            else
            {
                descriptionText.gameObject.SetActive(false);
            }

            levelChangeText.gameObject.GetComponent<TMP_Text>().text = levelNames[levelNumber];
        }

        private void Timer() {
            if (timeIsRunning) {
                timeRemaining += Time.deltaTime;
                DisplayTime(timeRemaining);
            }
        }

        private void ChangeDescriptionText(string text)
        {
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = text;
        }

        public void TakeDamage(float damage)
        {
            if (isInvincible) return;
            TakeDamageServerRpc(damage);
            damageScreen.GetComponent<Animator>().Play("Damage");
            audioSync.PlaySound(4);
        }
        
        private void Respawn()
        {
            levelChange.Play("LevelChange");
            levelChangeText.SetTrigger("LevelChange");
            StartCoroutine(ChangePlayerPos(0, spawnPoint));
            health.Value = 100;
            UpdateCursor(false);
        }

        private void Step()
        {
            if (isGrounded && IsOwner)
            {
                audioSync.PlaySound(audioClipNum.Value);
            }
        }

        private void DisplayTime(float timeToDisplay) {
            timeToDisplay += 1;
            int minutes = Mathf.FloorToInt(timeToDisplay / 60);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60);
            int milliseconds = Mathf.FloorToInt((timeToDisplay * 1000) % 1000);
            timeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }

        //Networking
        public override void OnNetworkSpawn()
        {
            SpawnObjects();
        }

        public struct NetworkNameVariable : INetworkSerializable
        {
            public string playerName;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref playerName);
            }
        }

        public void SpawnObjects() {
            if (!IsServer || !IsOwner) { return; }

            SpawnFire();
            SpawnDoors();
            SpawnGates();
            //SpawnMonsters();
            SpawnPotions();
        }

        public void SpawnFire()
        {
            GameObject[] torches = GameObject.FindGameObjectsWithTag("Torch");
            foreach (GameObject torch in torches)
            {
                if (UnityEngine.Random.value <= 0.5f)
                {
                    SpawnObjectServerRpc(torch.transform.GetChild(0).position, torch.transform.GetChild(0).rotation, 0);
                }
            }
        }

        public void SpawnDoors() {
            GameObject[] doorframes = GameObject.FindGameObjectsWithTag("DoorFrame");
            foreach (GameObject doorframe in doorframes)
            {
                SpawnObjectServerRpc(doorframe.transform.GetChild(0).position, doorframe.transform.GetChild(0).rotation, 1);
            }
        }

        public void SpawnGates() {
            GameObject[] gateframes = GameObject.FindGameObjectsWithTag("GateFrame");
            foreach (GameObject gateframe in gateframes)
            {
                SpawnObjectServerRpc(gateframe.transform.position, gateframe.transform.rotation, 2);
            }
        }

        public void SpawnOrbs(GameObject mushroom) {
            if (!IsServer) { return; }
            SpawnObjectServerRpc(mushroom.transform.position + new Vector3(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(2, 4), UnityEngine.Random.Range(-2, 2)), mushroom.transform.rotation, 3);
        }

        public void SpawnArrows(GameObject arrowTrap) {
            if (!IsServer) { return; }
            SpawnObjectServerRpc(arrowTrap.transform.position, arrowTrap.transform.rotation, 4);
        }

        public void SpawnStalagmites(GameObject stalagmite) {
            if (!IsServer) { return; }
            SpawnObjectServerRpc(stalagmite.transform.position, stalagmite.transform.rotation, 5);
        }

        public void SpawnMonsters()
        {
            GameObject[] slimes = GameObject.FindGameObjectsWithTag("Slime");
            foreach (GameObject slime in slimes)
            {
                SpawnObjectServerRpc(slime.transform.position, slime.transform.rotation, 6);
            }

            GameObject[] spiders = GameObject.FindGameObjectsWithTag("Spider");
            foreach (GameObject spider in spiders)
            {
                SpawnObjectServerRpc(spider.transform.position, spider.transform.rotation, 7);
            }

            GameObject[] rats = GameObject.FindGameObjectsWithTag("Rat");
            foreach (GameObject rat in rats)
            {
                SpawnObjectServerRpc(rat.transform.position, rat.transform.rotation, 8);
            }

            GameObject[] bats = GameObject.FindGameObjectsWithTag("Bat");
            foreach (GameObject bat in bats)
            {
                SpawnObjectServerRpc(bat.transform.position, bat.transform.rotation, 16);
            }

            GameObject[] snakes = GameObject.FindGameObjectsWithTag("Snake");
            foreach (GameObject snake in snakes)
            {
                SpawnObjectServerRpc(snake.transform.position, snake.transform.rotation, 17);
            }
        }

        public void SpawnPotions() {
            GameObject[] potions1 = GameObject.FindGameObjectsWithTag("Potion1");
            foreach (GameObject potion1 in potions1)
            {
                SpawnObjectServerRpc(potion1.transform.position, potion1.transform.rotation, 6);
            }

            GameObject[] potions2 = GameObject.FindGameObjectsWithTag("Potion2");
            foreach (GameObject potion2 in potions2)
            {
                SpawnObjectServerRpc(potion2.transform.position, potion2.transform.rotation, 7);
            }

            GameObject[] potions3 = GameObject.FindGameObjectsWithTag("Potion3");
            foreach (GameObject potion3 in potions3)
            {
                SpawnObjectServerRpc(potion3.transform.position, potion3.transform.rotation, 8);
            }

            GameObject[] potions4 = GameObject.FindGameObjectsWithTag("Potion4");
            foreach (GameObject potion4 in potions4)
            {
                SpawnObjectServerRpc(potion4.transform.position, potion4.transform.rotation, 9);
            }

            GameObject[] potions5 = GameObject.FindGameObjectsWithTag("Potion5");
            foreach (GameObject potion5 in potions5)
            {
                SpawnObjectServerRpc(potion5.transform.position, potion5.transform.rotation, 10);
            }
        }

        private bool IsPositionOccupied(Vector3 position, String tag)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag(tag))
                {
                    return true;
                }
            }
            return false;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnObjectServerRpc(Vector3 vector3, Quaternion rot, int prefNum)
        {
            SpawnObjectClientRpc(vector3, rot, prefNum);
        }

        [ClientRpc]
        public void SpawnObjectClientRpc(Vector3 vector3, Quaternion rot, int prefNum)
        {
            if (!IsServer) { return; }

            var obj = Instantiate(spawnPrefs[prefNum], vector3, rot);
            obj.GetComponent<NetworkObject>().Spawn(true);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeClipServerRpc(int clipNum) {
            ChangeClipClientRpc(clipNum);
        }

        [ClientRpc]
        private void ChangeClipClientRpc(int clipNum) {
            if (!IsServer) { return; }

            audioClipNum.Value = clipNum;
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangeSkinServerRpc() {
            ChangeSkinClientRpc();
        }

        [ClientRpc]
        private void ChangeSkinClientRpc() {
            if (!IsServer) { return; }

            //skinIndex.Value = settings.skinIndex;
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(float damage) {
            TakeDamageClientRpc(damage);
        }

        [ClientRpc]
        private void TakeDamageClientRpc(float damage) {
            health.Value -= damage;
        }

        private void ChangeSkin() {
            if (IsOwner)
                playerObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = armMeshMale[1];
            else
                playerObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = normalMeshMale[1];
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Projectile") {
                CollideServerRpc(collision.gameObject.GetComponent<TrapDamage>().damage);
                if (collision.gameObject.GetComponent<NetworkObject>().IsSpawned && IsServer) {
                    collision.gameObject.GetComponent<NetworkObject>().Despawn();
                }
            }    
        }

        [ServerRpc(RequireOwnership = false)]
        private void CollideServerRpc(float damage) {
            CollideClientRpc(damage);
        }

        [ClientRpc]
        private void CollideClientRpc(float damage) {
            TakeDamage(damage);
        }

        // IEnumerators
        private IEnumerator ChangePlayerPos(float waitTime, Vector3 vector3)
        {
            yield return new WaitForSeconds(waitTime);
            transform.position = vector3;
            gameObject.GetComponent<Rigidbody>().position = vector3;
        }

        private IEnumerator ChangeLevel(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            levelChange.Play("LevelChange");
            levelChangeText.SetTrigger("LevelChange");
            spawnPoint = transform.position;
        }

        private IEnumerator CanJump(float waitTime)
        {
            canJump = false;
            yield return new WaitForSeconds(waitTime);
            canJump = true;
        }

        private IEnumerator CanAttack(float waitTime)
        {
            canAttack = false;
            yield return new WaitForSeconds(waitTime);
            canAttack = true;
        }

        public IEnumerator TorchAttack() {
            audioSync.PlaySound(7);
            torch.GetComponent<Collider>().enabled = true;
            yield return new WaitForSeconds(0.5f);
            torch.GetComponent<Collider>().enabled = false;
        }

        private IEnumerator DrinkPotion(Potion potion) {
            var mainModle = potionParticle.main;
            if (potion.potionType == Potion.PotionType.Speed) {
                walkSpeed += 20;
                runSpeed += 20;
                mainModle.startColor = Color.blue;
            } else if (potion.potionType == Potion.PotionType.Healing) {
                health.Value = 100;
                mainModle.startColor = Color.yellow;
            } else if (potion.potionType == Potion.PotionType.Invincibility) {
                isInvincible = true;
                mainModle.startColor = Color.white;
            } else if (potion.potionType == Potion.PotionType.Strength) {
                torch.GetComponent<TrapDamage>().damage *= 2;
                mainModle.startColor = Color.red;
            } else if (potion.potionType == Potion.PotionType.Jumping) {
                jumpStrength *= 2;
                mainModle.startColor = Color.green;
            }
            yield return new WaitForSeconds(potion.duration);
            if (potion.potionType == Potion.PotionType.Speed) {
                walkSpeed -= 20;
                runSpeed -= 20;
            } else if (potion.potionType == Potion.PotionType.Healing) {
                health.Value = 100;
            } else if (potion.potionType == Potion.PotionType.Invincibility) {
                isInvincible = false;
            } else if (potion.potionType == Potion.PotionType.Strength) {
                torch.GetComponent<TrapDamage>().damage /= 2;
            } else if (potion.potionType == Potion.PotionType.Jumping) {
                jumpStrength /= 2;
            }
            mainModle.startColor = Color.black;
        }
    }
}

