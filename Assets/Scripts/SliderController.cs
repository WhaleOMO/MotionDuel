using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    // Start is called before the first frame update
    public float CountDown = 120f;
    private float curr; 
    
    private Slider _slider;
    private bool _isPlayingTickSound;
    
    public Image barImage;

    private void Start()
    {
        _slider = GetComponent<Slider>();
        curr = CountDown;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (curr < 0.5f)
        {
            Time.timeScale = 0f;
            GetComponent<AudioSource>().Pause();
        }
        
        if (curr < CountDown * 0.3)
        {
            var col = new Color(1, 0, 0, (float)Math.Sin(5*Time.fixedTime) + 0.2f);
            barImage.color = col;
            if (!_isPlayingTickSound)
            {
                _isPlayingTickSound = true;
                GetComponent<AudioSource>().Play();
            }
        }
        
        curr -= Time.fixedDeltaTime; 
        _slider.value = curr / CountDown;
    }
}
