
export function capturePointer(element, pointerId) {
    if (!element || !element.isConnected) {
        return;
    }

    if (!element.hasPointerCapture(pointerId)) {
        element.setPointerCapture(pointerId);
    }
}
