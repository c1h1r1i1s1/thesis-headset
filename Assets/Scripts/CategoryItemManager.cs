using UnityEngine;
using System.Collections.Generic;

public class CategoryItemManager : MonoBehaviour
{
    public GameObject CategoryItemPrefab;
    Dictionary<int, string> dict = new Dictionary<int, string>();

    private void setupDict(Dictionary<int, string> dict) {
        dict.Add(0, "person");
        dict.Add(1, "bicycle");
        dict.Add(2, "car");
        dict.Add(3, "motorcycle");
        dict.Add(4, "airplane");
        dict.Add(5, "bus");
        dict.Add(6, "train");
        dict.Add(7, "truck");
        dict.Add(8, "boat");
        dict.Add(9, "traffic light");
        dict.Add(10, "fire hydrant");
        dict.Add(11, "stop sign");
        dict.Add(12, "parking meter");
        dict.Add(13, "bench");
        dict.Add(14, "bird");
        dict.Add(15, "cat");
        dict.Add(16, "dog");
        dict.Add(17, "horse");
        dict.Add(18, "sheep");
        dict.Add(19, "cow");
        dict.Add(20, "elephant");
        dict.Add(21, "bear");
        dict.Add(22, "zebra");
        dict.Add(23, "giraffe");
        dict.Add(24, "backpack");
        dict.Add(25, "umbrella");
        dict.Add(26, "handbag");
        dict.Add(27, "tie");
        dict.Add(28, "suitcase");
        dict.Add(29, "frisbee");
        dict.Add(30, "skis");
        dict.Add(31, "snowboard");
        dict.Add(32, "sports ball");
        dict.Add(33, "kite");
        dict.Add(34, "baseball bat");
        dict.Add(35, "baseball glove");
        dict.Add(36, "skateboard");
        dict.Add(37, "surfboard");
        dict.Add(38, "tennis racket");
        dict.Add(39, "bottle");
        dict.Add(40, "wine glass");
        dict.Add(41, "cup");
        dict.Add(42, "fork");
        dict.Add(43, "knife");
        dict.Add(44, "spoon");
        dict.Add(45, "bowl");
        dict.Add(46, "banana");
        dict.Add(47, "apple");
        dict.Add(48, "sandwich");
        dict.Add(49, "orange");
        dict.Add(50, "broccoli");
        dict.Add(51, "carrot");
        dict.Add(52, "hot dog");
        dict.Add(53, "pizza");
        dict.Add(54, "donut");
        dict.Add(55, "cake");
        dict.Add(56, "chair");
        dict.Add(57, "couch");
        dict.Add(58, "potted plant");
        dict.Add(59, "bed");
        dict.Add(60, "dining table");
        dict.Add(61, "toilet");
        dict.Add(62, "tv");
        dict.Add(63, "laptop");
        dict.Add(64, "mouse");
        dict.Add(65, "remote");
        dict.Add(66, "keyboard");
        dict.Add(67, "cell phone");
        dict.Add(68, "microwave");
        dict.Add(69, "oven");
        dict.Add(70, "toaster");
        dict.Add(71, "sink");
        dict.Add(72, "refrigerator");
        dict.Add(73, "book");
        dict.Add(74, "clock");
        dict.Add(75, "vase");
        dict.Add(76, "scissors");
        dict.Add(77, "teddy bear");
        dict.Add(78, "hair drier");
        dict.Add(79, "toothbrush");
    }

    void Awake() {
        setupDict(dict);
    }
    
    void Start()
    {
        // Create 80 items
        for (int i=0; i<dict.Count; i++) {
            var categoryItem = Instantiate(CategoryItemPrefab, this.transform);
            var metadata = categoryItem.GetComponent<CategoryItem>();
            metadata.Init(i, dict[i]);

            var m_RectTransform = categoryItem.GetComponent<RectTransform>();
            m_RectTransform.offsetMin = new Vector2(-41.5f, m_RectTransform.offsetMin.y);
            m_RectTransform.offsetMax = new Vector2(m_RectTransform.offsetMax.x, -20*i);
        }
    }
}