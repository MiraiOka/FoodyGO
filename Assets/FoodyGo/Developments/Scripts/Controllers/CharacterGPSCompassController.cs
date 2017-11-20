using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using packt.FoodyGO.Mapping;
using packt.FoodyGO.Services;

namespace packt.FoodyGO.Controllers
{
	//GPS Location ServiceからGPSの値を読み取る
    public class CharacterGPSCompassController : MonoBehaviour
    {
        public GPSLocationService gpsLocationService;
        private double lastTimestamp;        
        private ThirdPersonCharacter thirdPersonCharacter;
        private Vector3 target;
        
        // Use this for initialization
        void Start()
        {
			//コンパスを利用する。
            Input.compass.enabled = true;
            thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
            if (gpsLocationService != null)
            {
                gpsLocationService.OnMapRedraw += GpsLocationService_OnMapRedraw;
            }
        }

        private void GpsLocationService_OnMapRedraw(GameObject g)
        {
            transform.position = Vector3.zero;
            target = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            if (gpsLocationService != null &&
                gpsLocationService.IsServiceStarted &&
                gpsLocationService.PlayerTimestamp > lastTimestamp)
            {
                //convert GPS lat/long to world x/y
				//GPSの緯度・経度をワールドのx・yに変換
                var x = ((GoogleMapUtils.LonToX(gpsLocationService.Longitude)
                    - gpsLocationService.mapWorldCenter.x) * gpsLocationService.mapScale.x);
                var y = (GoogleMapUtils.LatToY(gpsLocationService.Latitude)
                    - gpsLocationService.mapWorldCenter.y) * gpsLocationService.mapScale.y;
                target = new Vector3(-x, 0, y);
            }

            //check if the character has reached the new point
			//キャラクターが新しい点に到着したかどうか
            if (Vector3.Distance(target, transform.position) > .025f)
            {
                var move = target - transform.position;
                thirdPersonCharacter.Move(move, false, false);
            }
            else
            {
                //stop moving
                thirdPersonCharacter.Move(Vector3.zero, false, false);

                // Orient an object to point to magnetic north and adjust for map reversal
				// オブジェクトを磁北に向けて、反転しているマップの向きに合わせる。
                var heading = 180 + Input.compass.magneticHeading;

				//Vector3.up(上向き)の軸でheadingだけ開店したrotation
                var rotation = Quaternion.AngleAxis(heading, Vector3.up);

				//LerpとSlerpの違いは62ページ参照
				//Time.fixedTime→1フレームあたりの時間
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedTime * .001f);
            }
        }
    }
}
