using UnityEngine;


namespace LabyrinthGenerator
{
    /// <summary>
    /// Add this script on Game Object
    /// </summary>
    public class Labyrinth : MonoBehaviour
    {


        [SerializeField] private GameObject _cellPrefub;
        [SerializeField] private GameObject _wallPrefub;
        [SerializeField] private int _labyrinthSizeX;
        [SerializeField] private int _labyrinthSizeZ;


        private void Start()
        {

            LabyrinthGenerator labyrinth = new LabyrinthGenerator(
                transform,
                _labyrinthSizeX,
                _labyrinthSizeZ,
                _cellPrefub,
                _wallPrefub
                );

            labyrinth.GenerateLabyrinth();
        }


    }
}
