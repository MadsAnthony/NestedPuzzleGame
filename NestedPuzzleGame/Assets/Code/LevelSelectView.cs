using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectView : MonoBehaviour
{
   [SerializeField] private Camera camera;
   [SerializeField] private List<GameObject> listOfLevels;
   [SerializeField] private AnimationCurve easeInOutCurve;

   public int currentLevelIndex;
   private void Start() {
      currentLevelIndex = Director.Instance.LevelIndex;
      if (currentLevelIndex>0) {
         StartCoroutine(InitialFlow());
      }
   }

   private IEnumerator InitialFlow() {
      yield return MoveToLevelCr(currentLevelIndex, 1);
      yield return new WaitForSeconds(1);
      MoveRight();
   }

   public void MoveLeft() {
      currentLevelIndex -= 1;
      currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, listOfLevels.Count - 1);
      StartCoroutine(MoveToLevelCr(currentLevelIndex, 0.05f));
   }
   
   public void MoveRight() {
      currentLevelIndex += 1;
      currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, listOfLevels.Count - 1);
      StartCoroutine(MoveToLevelCr(currentLevelIndex, 0.05f));
   }

   private IEnumerator MoveToLevelCr(int levelIndex, float timeIncrement) {
      var endPosition = gameObject.transform.position - listOfLevels[currentLevelIndex].transform.position;
      yield return AnimateTo(gameObject, endPosition, timeIncrement);
   }
   
   private IEnumerator AnimateTo(GameObject gameObject, Vector3 endPosition, float timeIncrement) {
      var startPosition = gameObject.transform.position;
      float time = 0;
      while (time < 1) {
         time += timeIncrement;
         var t = easeInOutCurve.Evaluate (time);
         gameObject.transform.position = (startPosition * (1 - t)) + endPosition * t;
         yield return null;
      }
   }
}
