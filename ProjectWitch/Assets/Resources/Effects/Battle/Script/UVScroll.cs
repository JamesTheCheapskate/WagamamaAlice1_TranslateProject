﻿using UnityEngine;
using System.Collections;

namespace ProjectWitch
{
    public class UVScroll : MonoBehaviour
    {

        [SerializeField]
        private float scrollSpeedX = 0.1f;

        [SerializeField]
        private float scrollSpeedY = 0.1f;

        int flame = 0;

        void Start()
        {
            GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", Vector2.zero);
        }

        void Update()
        {
            if (flame <= 50 && flame >= 35)
            {
                var x = Mathf.Repeat(Time.time * scrollSpeedX, 1);
                var y = Mathf.Repeat(Time.time * scrollSpeedY, 1);


                var offset = new Vector2(x, y);



                GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);
            }
            flame++;
        }
    }
}