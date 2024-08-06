using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AC
{
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Dungeon Generator Settings")]
        public int dungeonSeed;
        public Vector2Int dungeonSize;
        public Vector2 dungeonOffset;
        public int dungeonLevels;
        public GameObject[] firstRooms;
        public GameObject[] finalRooms;
        public GameObject[] fillerRooms;
        public Room[] rooms;

        private List<Cell> board;
        private Vector2 newDungeonOffset;
        private GameObject firstGeneratedRoom;
        private float dungeonYOffset;
        private int levelIndex;
        private bool isPlaying;

        public class Cell
        {
            public bool visited = false;
            public bool[] status = new bool[4];
        }

        [System.Serializable]
        public class Room
        {
            public GameObject room;
            public Vector2Int minPosition;
            public Vector2Int maxPosition;
            [Range(0,1)] public float spawnProbability;
            [Range(0,9)] public int levelToSpawn;
            public bool obligatory;

            public int ProbabilityOfSpawning(int x, int y)
            {
                if (x >= minPosition.x && x <= maxPosition.x && y >= minPosition.y && y <= maxPosition.y)
                {
                    return obligatory ? 2 : 1;
                }
                return 0;
            }
        }

        private void Start()
        {
            isPlaying = true;

            if (LobbySaver.instance != null)
                dungeonSeed = LobbySaver.instance.networkSeed.Value;

            GenerateDungeon();
        }

        public void GenerateDungeon()
        {
            ClearRooms();
            GenerateMaze();
            GenerateRooms();
        }

        public void ClearRooms() {
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                if (isPlaying) 
                {
                    Destroy(gameObject.transform.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
                }
            }
        }

        public void GenerateMaze()
        {
            Random.InitState(dungeonSeed);

            board = new List<Cell>();

            for (int i = 0; i < dungeonSize.x; i++)
            {
                for (int j = 0; j < dungeonSize.y; j++)
                {
                    board.Add(new Cell());
                }
            }

            int currentCell = 0;
            Stack<int> path = new Stack<int>();
            int k = 0;

            while (k < 1000)
            {
                k++;

                board[currentCell].visited = true;

                if (currentCell == board.Count - 1)
                {
                    break;
                }

                List<int> neighbors = CheckNeighbors(currentCell);

                if (neighbors.Count == 0)
                {
                    if (path.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        currentCell = path.Pop();
                    }
                }
                else
                {
                    path.Push(currentCell);

                    int newCell = neighbors[Random.Range(0, neighbors.Count)];

                    if (newCell > currentCell)
                    {
                        if (newCell - 1 == currentCell)
                        {
                            board[currentCell].status[2] = true;
                            currentCell = newCell;
                            board[currentCell].status[3] = true;
                        }
                        else
                        {
                            board[currentCell].status[1] = true;
                            currentCell = newCell;
                            board[currentCell].status[0] = true;
                        }
                    }
                    else
                    {
                        if (newCell + 1 == currentCell)
                        {
                            board[currentCell].status[3] = true;
                            currentCell = newCell;
                            board[currentCell].status[2] = true;
                        }
                        else
                        {
                            board[currentCell].status[0] = true;
                            currentCell = newCell;
                            board[currentCell].status[1] = true;
                        }
                    }
                }
            }
        }

        public void GenerateRooms() {
            for (int i = 0; i < dungeonSize.x; i++)
            {
                for (int j = 0; j < dungeonSize.y; j++)
                {
                    Cell currentCell = board[i + j * dungeonSize.x];
                    if (currentCell.visited)
                    {
                        int randomRoom = -1;
                        List<int> availableRooms = new List<int>();

                        for (int k = 0; k < rooms.Length; k++)
                        {
                            int p = rooms[k].ProbabilityOfSpawning(i, j);

                            if (p == 2 && rooms[k].levelToSpawn == levelIndex)
                            {
                                randomRoom = k;
                                break;
                            }
                            else if (p == 1 && rooms[k].levelToSpawn <= levelIndex)
                            {
                                availableRooms.Add(k);
                            }
                        }

                        if (randomRoom == -1)
                        {
                            if (availableRooms.Count > 0)
                            {
                                float[] probabilities = availableRooms.Select(k => rooms[k].spawnProbability).ToArray();
                                randomRoom = availableRooms[ChooseRoomWithProbability(probabilities)];
                            }
                            else
                            {
                                randomRoom = 0;
                            }
                        }

                        if (rooms[randomRoom].room == null) { return; }

                        if (rooms[randomRoom].levelToSpawn == levelIndex)
                        {
                            var newRoom = Instantiate(rooms[randomRoom].room, new Vector3(i * dungeonOffset.x + transform.position.x + newDungeonOffset.x, transform.position.y + dungeonYOffset, -j * dungeonOffset.y + transform.position.z + newDungeonOffset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                            newRoom.UpdateRoom(currentCell.status);
                            newRoom.name += " " + i + "-" + j + " Level " + levelIndex;

                            if (firstGeneratedRoom == null)
                            {
                                firstGeneratedRoom = newRoom.gameObject;
                            }
                        } else {
                            var newRoom = Instantiate(fillerRooms[levelIndex], new Vector3(i * dungeonOffset.x + transform.position.x + newDungeonOffset.x, transform.position.y + dungeonYOffset, -j * dungeonOffset.y + transform.position.z + newDungeonOffset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                            newRoom.UpdateRoom(currentCell.status);
                            newRoom.name += " " + i + "-" + j + " Level " + levelIndex;
                            if (firstGeneratedRoom == null)
                            {
                                firstGeneratedRoom = newRoom.gameObject;
                            }
                        }
                    }
                }
            }

            List<GameObject> generatedRooms = new List<GameObject>();
            foreach (Transform child in transform)
            {
                generatedRooms.Add(child.gameObject);
            }

            GameObject firstRoom = firstGeneratedRoom;
            GameObject finalRoom = generatedRooms[generatedRooms.Count - 1];
            if (dungeonLevels == levelIndex + 1) {
                GameObject newFirstRoom = Instantiate(firstRooms[finalRooms.Length - 1], firstRoom.transform.position, Quaternion.identity, transform);
                newFirstRoom.GetComponent<RoomBehaviour>().UpdateRoom(firstRoom.GetComponent<RoomBehaviour>().GetStatus());
                newFirstRoom.name = "Start Room Level" + levelIndex;
                
                GameObject newFinalRoom = Instantiate(finalRooms[finalRooms.Length - 1], finalRoom.transform.position, Quaternion.identity, transform);
                newFinalRoom.GetComponent<RoomBehaviour>().UpdateRoom(finalRoom.GetComponent<RoomBehaviour>().GetStatus());
                newFinalRoom.name = "Final Room Level" + levelIndex;
            }
            else 
            {
                GameObject newFirstRoom = Instantiate(firstRooms[levelIndex], firstRoom.transform.position, Quaternion.identity, transform);
                newFirstRoom.GetComponent<RoomBehaviour>().UpdateRoom(firstRoom.GetComponent<RoomBehaviour>().GetStatus());
                newFirstRoom.name = "Start Room Level " + levelIndex;

                GameObject newFinalRoom = Instantiate(finalRooms[levelIndex], finalRoom.transform.position, Quaternion.identity, transform);
                newFinalRoom.GetComponent<RoomBehaviour>().UpdateRoom(finalRoom.GetComponent<RoomBehaviour>().GetStatus());
                newFinalRoom.name = "Level Room Level " + levelIndex;
                
                dungeonYOffset -= 15;
                levelIndex++;
                dungeonSeed++;
                newDungeonOffset.x = newFinalRoom.transform.position.x;
                newDungeonOffset.y = newFinalRoom.transform.position.z;
                firstGeneratedRoom = null;
                GenerateMaze();
                GenerateRooms();     
            }

            if (isPlaying) 
            {
                Destroy(firstRoom);
                Destroy(finalRoom);
            } 
            else 
            {
                DestroyImmediate(firstRoom);
                DestroyImmediate(finalRoom);
            }
        }

        private int ChooseRoomWithProbability(float[] probabilities)
        {
            float totalProbability = probabilities.Sum();

            // Normalize probabilities
            for (int i = 0; i < probabilities.Length; i++)
            {
                probabilities[i] /= totalProbability;
            }

            float randomValue = Random.value;
            
            float cumulativeProbability = 0f;
            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulativeProbability += probabilities[i];
                if (randomValue <= cumulativeProbability)
                {
                    return i;
                }
            }

            return probabilities.Length - 1;
        }

        private List<int> CheckNeighbors(int cell)
        {
            List<int> neighbors = new List<int>();

            if (cell - dungeonSize.x >= 0 && !board[(cell - dungeonSize.x)].visited)
            {
                neighbors.Add((cell - dungeonSize.x));
            }

            if (cell + dungeonSize.x < board.Count && !board[(cell + dungeonSize.x)].visited)
            {
                neighbors.Add((cell + dungeonSize.x));
            }

            if ((cell + 1) % dungeonSize.x != 0 && !board[(cell + 1)].visited)
            {
                neighbors.Add((cell + 1));
            }

            if (cell % dungeonSize.x != 0 && !board[(cell - 1)].visited)
            {
                neighbors.Add((cell - 1));
            }

            return neighbors;
        }
    }
}