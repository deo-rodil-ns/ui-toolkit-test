using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sylpheed.Extensions
{
    public static class EventSystemExtensions
    {
        public static bool IsPointerOverUIElement(this EventSystem eventSystem)
        {
            if (!eventSystem.IsPointerOverGameObject()) return false;
			
            var pointer = new PointerEventData(eventSystem);
            pointer.position = Input.mousePosition;

            var results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointer, results);

            return results.Any(r => r.gameObject.transform is RectTransform);
        }
    }
}