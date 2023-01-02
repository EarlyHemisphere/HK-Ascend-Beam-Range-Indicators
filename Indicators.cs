using UnityEngine;
using Logger = Modding.Logger;

namespace AscendBeamIndicator {
    internal class Indicators : MonoBehaviour {
        private PlayMakerFSM _attackCommands;
        private PlayMakerFSM _ascendBeamControl = null;
        private GameObject _knight;
        private GameObject _eyeBeamGlow = null;
        private GameObject _ascendBeam = null;
        private GameObject _leftBound;
        private LineRenderer _leftBoundRenderer;
        private GameObject _rightBound;
        private LineRenderer _rightBoundRenderer;
        private GameObject _prevLeftBound;
        private LineRenderer _prevLeftBoundRenderer;
        private GameObject _prevRightBound;
        private LineRenderer _prevRightBoundRenderer;
        private bool _linesInitialized = false;
        private Vector3 _beamWidthOffsetLeft = new Vector3(0.6f, 0.6f, 0); // these are approximate and were found by trial and error
        private Vector3 _beamWidthOffsetRight = new Vector3(-0.6f, 0.6f, 0);
        private bool _beamAnticStarted = false;
        private bool _ascensionCompleted = false;

        private void Awake() {
            Log("Added BeamRangeIndicators MonoBehaviour");

            _attackCommands = gameObject.LocateMyFSM("Attack Commands");
            _knight = GameObject.Find("Knight");

            Material mat = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            mat.SetColor("_Color", Color.red);

            _leftBound = new GameObject();
            _leftBound.AddComponent<LineRenderer>();
            _leftBoundRenderer = _leftBound.GetComponent<LineRenderer>();
            createLine(_leftBoundRenderer, mat, Color.yellow);

            _rightBound = new GameObject();
            _rightBound.AddComponent<LineRenderer>();
            _rightBoundRenderer = _rightBound.GetComponent<LineRenderer>();
            createLine(_rightBoundRenderer, mat, Color.yellow);

            _prevLeftBound = new GameObject();
            _prevLeftBound.AddComponent<LineRenderer>();
            _prevLeftBoundRenderer = _prevLeftBound.GetComponent<LineRenderer>();
            createLine(_prevLeftBoundRenderer, mat, Color.red);

            _prevRightBound = new GameObject();
            _prevRightBound.AddComponent<LineRenderer>();
            _prevRightBoundRenderer = _prevRightBound.GetComponent<LineRenderer>();
            createLine(_prevRightBoundRenderer, mat, Color.red);
        }

        private void Update() {
            try {
                if (_ascendBeam == null) {
                    _ascendBeam = GameObject.Find("Ascend Beam");
                }

                if (_eyeBeamGlow == null) {
                    _eyeBeamGlow = GameObject.Find("Eye Beam Glow");
                }

                if (gameObject.transform.position.y >= 150f && _eyeBeamGlow != null && _ascendBeam != null && !_ascensionCompleted) {
                    // Ascension has begun and Radiance has started firing beams

                    if (_knight.transform.position.y >= 150f) {
                        destroyLines();
                        _ascensionCompleted = true;
                    } else {
                        if (_ascendBeamControl == null) {
                            _ascendBeamControl = _ascendBeam.LocateMyFSM("Control");
                        }

                        if (!_linesInitialized) {
                            Log("Ascension has begun, adding range indicators");
                            _leftBoundRenderer.SetPosition(0, _eyeBeamGlow.transform.position + _beamWidthOffsetLeft);
                            _rightBoundRenderer.SetPosition(0, _eyeBeamGlow.transform.position + _beamWidthOffsetRight);
                            _prevLeftBoundRenderer.SetPosition(0, _eyeBeamGlow.transform.position + _beamWidthOffsetLeft);
                            _prevRightBoundRenderer.SetPosition(0, _eyeBeamGlow.transform.position + _beamWidthOffsetRight);
                            _linesInitialized = true;
                        }

                        updateIndicators();
                    }
                }
            } catch (System.NullReferenceException) {
                // Radiance is not firing beams yet
            } catch (System.Exception e) {
                Log("Could not draw indicators: " + e);
            }
        }

        private void createLine(LineRenderer renderer, Material mat, Color color) {
            renderer.material = mat;
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.startWidth = 0.15f;
            renderer.endWidth = 0.15f;
        }

        private void updateIndicators() {
            Vector3 knightPos = _knight.transform.position;
            Vector3 eyeBeamGlowPos = _eyeBeamGlow.transform.position;

            _leftBoundRenderer.SetPosition(1, Quaternion.Euler(0, 0, 5) * (knightPos - new Vector3(0, -0.5f, 0) - eyeBeamGlowPos) + eyeBeamGlowPos + _beamWidthOffsetLeft);
            _rightBoundRenderer.SetPosition(1, Quaternion.Euler(0, 0, -5) * (knightPos - new Vector3(0, -0.5f, 0) - eyeBeamGlowPos) + eyeBeamGlowPos + _beamWidthOffsetRight);

            // Extend left bound indicator past the player
            Vector3[] leftBoundPositions = new Vector3[2];
            _leftBoundRenderer.GetPositions(leftBoundPositions);
            float leftBoundSlope = (leftBoundPositions[0].x - leftBoundPositions[1].x) / (leftBoundPositions[0].y - leftBoundPositions[1].y);
            _leftBoundRenderer.SetPosition(1, new Vector3(leftBoundPositions[1].x - 100 * leftBoundSlope, leftBoundPositions[1].y - 100));

            // Extend right bound indicator past the player
            Vector3[] rightBoundPositions = new Vector3[2];
            _rightBoundRenderer.GetPositions(rightBoundPositions);
            float rightBoundSlope = (rightBoundPositions[0].x - rightBoundPositions[1].x) / (rightBoundPositions[0].y - rightBoundPositions[1].y);
            _rightBoundRenderer.SetPosition(1, new Vector3(rightBoundPositions[1].x - 100 * rightBoundSlope, rightBoundPositions[1].y - 100));

            if (_ascendBeamControl.ActiveStateName == "Antic") {
                if (!_beamAnticStarted) {
                    _prevLeftBoundRenderer.SetPosition(1, _leftBoundRenderer.GetPosition(1) + _beamWidthOffsetLeft);
                    _prevRightBoundRenderer.SetPosition(1, _rightBoundRenderer.GetPosition(1) + _beamWidthOffsetRight);
                    _beamAnticStarted = true;
                }
            } else {
                _beamAnticStarted = false;
            }
        }

        private void destroyLines() {
            Destroy(_leftBound);
            Destroy(_rightBound);
            Destroy(_prevLeftBound);
            Destroy(_prevRightBound);
        }

        private static void Log(object obj) {
            Logger.Log("[AscendBeamIndicator] - " + obj);
        }
    }
}