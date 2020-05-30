using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


namespace LabyrinthGenerator
{
    public class LabyrinthGenerator : EditorWindow
    {


        #region Fields

        private const string NOTICE = "First needs to fill required fields";

        private Cell[,] _cells;
        private GameObject _parent;
        private GameObject _cellPrefub;
        private GameObject _wallPrefub;
        private GUIStyle style = new GUIStyle();
        private float _cellGridSizeX;
        private float _cellGridSizeZ;
        private int _labyrinthSizeX;
        private int _labyrinthSizeZ;

        #endregion


        #region PrivateData

        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        #endregion


        #region Window

        [MenuItem("Tools/Labyrinth Generator")]
        public static void ShowWindow()
        {
            GetWindow(typeof(LabyrinthGenerator), false, "Labyrinth Generator");
        }

        private void OnEnable()
        {
            style.richText = true;
            style.alignment = TextAnchor.MiddleCenter;
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.Label("Labyrinth size in prefubs (required)", EditorStyles.boldLabel);
            _labyrinthSizeX = EditorGUILayout.IntField("X", _labyrinthSizeX);
            _labyrinthSizeZ = EditorGUILayout.IntField("Z", _labyrinthSizeZ);

            GUILayout.Space(10);
            GUILayout.Label("Road prefub (required)", EditorStyles.boldLabel);
            _cellPrefub = EditorGUILayout.ObjectField(_cellPrefub, typeof(GameObject), true) as GameObject;

            GUILayout.Space(10);
            GUILayout.Label("Wall prefub (optional)", EditorStyles.boldLabel);
            _wallPrefub = EditorGUILayout.ObjectField(_wallPrefub, typeof(GameObject), true) as GameObject;

            GUILayout.Space(10);
            GUILayout.Label("Parent GameObject on Scene (optional)", EditorStyles.boldLabel);
            _parent = EditorGUILayout.ObjectField(_parent, typeof(GameObject), true) as GameObject;

            GUILayout.Space(10);
            var notice = CheckFills() == false ? $"<color=red>{NOTICE}</color>" : String.Empty;
            GUILayout.Label(notice, style);

            GUILayout.Space(10);
            if (GUILayout.Button("Generate") && CheckFills())
                GenerateLabyrinth();
        }

        #endregion


        #region Methods

        private void GenerateLabyrinth()
        {
            if (_parent == null)
                _parent = new GameObject { name = "Labyrinth" };

            _cells = new Cell[_labyrinthSizeX, _labyrinthSizeZ];
            Stack<Cell> cellsStack = new Stack<Cell>();

            Cell currentSell = new Cell(_labyrinthSizeX / 2, _labyrinthSizeZ / 2, _parent.transform.position, true);
            var firstObj = Instantiate(_cellPrefub, currentSell.Position, Quaternion.identity) as GameObject;
            firstObj.transform.SetParent(_parent.transform);
            SetGridStepsSize(firstObj);

            do
            {
                var rowX = currentSell.RowX;
                var columnZ = currentSell.ColumnZ;

                List<Cell> cellsForNextStep = new List<Cell>();

                // right cell
                if (CheckCell(rowX + 1, columnZ) && CheckPlaceAroundCell(rowX + 1, columnZ, Direction.Right))
                    cellsForNextStep.Add(new Cell(rowX + 1, columnZ, SetPosition(currentSell.Position, Direction.Right)));

                // left cell
                if (CheckCell(rowX - 1, columnZ) && CheckPlaceAroundCell(rowX - 1, columnZ, Direction.Left))
                    cellsForNextStep.Add(new Cell(rowX - 1, columnZ, SetPosition(currentSell.Position, Direction.Left)));

                // up cell
                if (CheckCell(rowX, columnZ + 1) && CheckPlaceAroundCell(rowX, columnZ + 1, Direction.Up))
                    cellsForNextStep.Add(new Cell(rowX, columnZ + 1, SetPosition(currentSell.Position, Direction.Up)));

                // down cell
                if (CheckCell(rowX, columnZ - 1) && CheckPlaceAroundCell(rowX, columnZ - 1, Direction.Down))
                    cellsForNextStep.Add(new Cell(rowX, columnZ - 1, SetPosition(currentSell.Position, Direction.Down)));

                if (cellsForNextStep.Count > 0)
                {
                    Cell newCell = cellsForNextStep[Random.Range(0, cellsForNextStep.Count)];

                    _cells[newCell.RowX, newCell.ColumnZ] = newCell;
                    _cells[newCell.RowX, newCell.ColumnZ].IsBusy = true;
                    cellsStack.Push(_cells[newCell.RowX, newCell.ColumnZ]);
                    currentSell = _cells[newCell.RowX, newCell.ColumnZ];

                    var cellObj = Instantiate(_cellPrefub, newCell.Position, Quaternion.identity) as GameObject;
                    cellObj.transform.SetParent(_parent.transform);
                }
                else
                {
                    currentSell = cellsStack.Pop();
                }
            }
            while (cellsStack.Count > 0);


            if (_wallPrefub != null)
            {
                // идем по массиву селл по строкам
                // если ячейка не Бизи
                //то ищем рядом бизи ячейку и рассчитываем где поставить стену
            }
        }

        private void SetGridStepsSize(GameObject gameObj)
        {
            var boxCollider = gameObj.GetComponent<BoxCollider>();
            if (boxCollider == null)
                gameObj.AddComponent<BoxCollider>();
            _cellGridSizeX = boxCollider.bounds.size.x;
            _cellGridSizeZ = boxCollider.bounds.size.z;
        }

        private bool CheckPlaceAroundCell(int rowX, int columnZ, Direction value)
        {
            switch (value)
            {
                case Direction.Up:
                    for (var x = -1; x < 2; x++)
                    {
                        for (var z = 0; z < 2; z++)
                        {
                            if (!CheckCell(rowX + x, columnZ + z))
                                return false;
                        }
                    }
                    return true;

                case Direction.Down:
                    for (var x = -1; x < 2; x++)
                    {
                        for (var z = -1; z < 1; z++)
                        {
                            if (!CheckCell(rowX + x, columnZ + z))
                                return false;
                        }
                    }
                    return true;

                case Direction.Left:
                    for (var x = -1; x < 1; x++)
                    {
                        for (var z = -1; z < 2; z++)
                        {
                            if (!CheckCell(rowX + x, columnZ + z))
                                return false;
                        }
                    }
                    return true;

                case Direction.Right:
                    for (var x = 0; x < 2; x++)
                    {
                        for (var z = -1; z < 2; z++)
                        {
                            if (!CheckCell(rowX + x, columnZ + z))
                                return false;
                        }
                    }
                    return true;
            }
            return false;
        }

        private bool CheckCell(int rowX, int columnZ)
        {
            if (CheckInArray(rowX, columnZ))
                return _cells[rowX, columnZ].IsBusy == true ? false : true;
            else
                return false;
        }

        private bool CheckInArray(int rowX, int columnZ)
        {
            return rowX >= 0 && rowX < _cells.GetLength(0) && columnZ >= 0 && columnZ < _cells.GetLength(1);
        }

        private Vector3 SetPosition(Vector3 currentPosition, Direction value)
        {
            var newPosition = currentPosition;
            switch (value)
            {
                case Direction.Up:
                    newPosition.z += _cellGridSizeZ;
                    break;
                case Direction.Down:
                    newPosition.z -= _cellGridSizeZ;
                    break;
                case Direction.Left:
                    newPosition.x -= _cellGridSizeX;
                    break;
                case Direction.Right:
                    newPosition.x += _cellGridSizeX;
                    break;
            }
            return newPosition;
        }

        private bool CheckFills()
        {
            return _cellPrefub != null
                && _labyrinthSizeX > 0
                && _labyrinthSizeZ > 0;
        }

        #endregion


    }
}
