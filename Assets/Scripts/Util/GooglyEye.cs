// MIT License
// 
// Copyright (c) 2019 Erik Skoglund
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Modifications 
//
// Copyright (c) 2025 Keiwan Donyagard

using UnityEngine;

public class GooglyEye : MonoBehaviour
{
    public Transform Eye;
    [Range(0.5f, 10f)]
    public float Speed = 1f;
    [Range(0f, 5f)]
    public float GravityMultiplier = 1f;
    [Range(0.01f, 0.98f)]
    public float Bounciness = 0.4f;

    private Vector3 _origin;
    private Vector3 _velocity;
    private Vector3 _lastPosition;

    public SpriteRenderer[] spriteRenderers;

    void Start() {
        _origin = Eye.localPosition;
        _lastPosition = transform.position;

		spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void Update() {
        const float maxDistance = 0.25f;

        var currentPosition = transform.position;

        var gravity = transform.InverseTransformDirection(Physics.gravity);

        _velocity += gravity * GravityMultiplier * Time.deltaTime;
        _velocity += transform.InverseTransformVector(_lastPosition - currentPosition) * 500f * Time.deltaTime;
        _velocity.z = 0f;

        var position = Eye.localPosition;

        position += _velocity * Speed * Time.deltaTime;

        var direction = new Vector2(position.x, position.y);
        var angle = Mathf.Atan2(direction.y, direction.x);

        if (direction.magnitude > maxDistance) {
            var normal = -direction.normalized;

            _velocity = Vector2.Reflect(new Vector2(_velocity.x, _velocity.y), normal) * Bounciness;
            
            position = new Vector3(
                Mathf.Cos(angle) * maxDistance,
                Mathf.Sin(angle) * maxDistance,
                0f
            );
        }

        position.z = Eye.localPosition.z;
        Eye.localPosition = position;
        _lastPosition = transform.position;
    }
}