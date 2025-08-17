export class abstCanvas {
    static getContext(canvas, pixilated) {
        const ctx = canvas.getContext('2d');
        if (pixilated) {
            ctx.imageSmoothingEnabled = false;
        }
        return ctx;
    }

    static clear(ctx, color, width, height) {
        ctx.fillStyle = color;
        ctx.fillRect(0, 0, width, height);
    }

    static setPixel(ctx, x, y, color) {
        ctx.fillStyle = color;
        ctx.fillRect(x, y, 1, 1);
    }

    static drawLine(ctx, x1, y1, x2, y2, color, width) {
        ctx.beginPath();
        ctx.moveTo(x1, y1);
        ctx.lineTo(x2, y2);
        ctx.strokeStyle = color;
        ctx.lineWidth = width;
        ctx.stroke();
    }

    static drawRect(ctx, x, y, w, h, color, filled, width) {
        if (filled) {
            ctx.fillStyle = color;
            ctx.fillRect(x, y, w, h);
        } else {
            ctx.strokeStyle = color;
            ctx.lineWidth = width;
            ctx.strokeRect(x, y, w, h);
        }
    }

    static drawCircle(ctx, x, y, radius, color, filled, width) {
        ctx.beginPath();
        ctx.arc(x, y, radius, 0, Math.PI * 2);
        if (filled) {
            ctx.fillStyle = color;
            ctx.fill();
        } else {
            ctx.strokeStyle = color;
            ctx.lineWidth = width;
            ctx.stroke();
        }
    }

    static drawArc(ctx, x, y, radius, startDeg, endDeg, color, width) {
        ctx.beginPath();
        ctx.arc(x, y, radius, startDeg * Math.PI / 180, endDeg * Math.PI / 180);
        ctx.strokeStyle = color;
        ctx.lineWidth = width;
        ctx.stroke();
    }

    static drawPolygon(ctx, points, color, filled, width) {
        if (points.length < 4) return;
        ctx.beginPath();
        ctx.moveTo(points[0], points[1]);
        for (let i = 2; i < points.length; i += 2) {
            ctx.lineTo(points[i], points[i + 1]);
        }
        ctx.closePath();
        if (filled) {
            ctx.fillStyle = color;
            ctx.fill();
        } else {
            ctx.strokeStyle = color;
            ctx.lineWidth = width;
            ctx.stroke();
        }
    }

    static drawText(ctx, x, y, text, font, color, fontSize, alignment) {
        ctx.fillStyle = color;
        ctx.font = font || (fontSize + 'px sans-serif');
        ctx.textAlign = alignment;
        const lines = text.split('\n');
        for (let i = 0; i < lines.length; i++) {
            ctx.fillText(lines[i], x, y + i * fontSize);
        }
    }

    static drawPictureData(ctx, data, width, height, x, y) {
        const imgData = new ImageData(new Uint8ClampedArray(data), width, height);
        ctx.putImageData(imgData, x, y);
    }
}

export class AbstUIKey {
    static setCursor(cursor) {
        document.body.style.cursor = cursor;
    }
}

export class AbstUIWindow {
    static showBootstrapModal(id) {
        const el = document.getElementById(id);
        if (!el) return;
        const modal = bootstrap.Modal.getOrCreateInstance(el);
        modal.show();
    }

    static hideBootstrapModal(id) {
        const el = document.getElementById(id);
        if (!el) return;
        const modal = bootstrap.Modal.getOrCreateInstance(el);
        modal.hide();
    }
}
