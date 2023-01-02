using UnityEngine;
using Logger = Modding.Logger;
using ModCommon.Util;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace AscendBeamRangeIndicators {
    internal class Indicators : MonoBehaviour {
        private PlayMakerFSM attackChoices;
        private PlayMakerFSM attackCommands;
        private PlayMakerFSM ascendBeamControl = null;
        private GameObject knight;
        private GameObject eyeBeamGlow = null;
        private GameObject ascendBeam = null;
        private GameObject leftBound;
        private LineRenderer leftBoundRenderer;
        private GameObject rightBound;
        private LineRenderer rightBoundRenderer;
        private GameObject prevLeftBound;
        private LineRenderer prevLeftBoundRenderer;
        private GameObject prevRightBound;
        private LineRenderer prevRightBoundRenderer;
        private bool linesInitialized = false;
        private Vector3 beamWidthOffsetLeft = new Vector3(0.6f, 0.6f, 0); // these are approximate and were found by trial and error
        private Vector3 beamWidthOffsetRight = new Vector3(-0.6f, 0.6f, 0);
        private bool ascensionCompleted = false;

        private void Awake() {
            Log("Added BeamRangeIndicators MonoBehaviour");

            attackCommands = gameObject.LocateMyFSM("Attack Commands");
            attackChoices = gameObject.LocateMyFSM("Attack Choices");
            knight = GameObject.Find("Knight");

            Material mat = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

            leftBound = new GameObject();
            leftBound.AddComponent<LineRenderer>();
            leftBoundRenderer = leftBound.GetComponent<LineRenderer>();
            createLine(leftBoundRenderer, mat, Color.yellow);

            rightBound = new GameObject();
            rightBound.AddComponent<LineRenderer>();
            rightBoundRenderer = rightBound.GetComponent<LineRenderer>();
            createLine(rightBoundRenderer, mat, Color.yellow);

            prevLeftBound = new GameObject();
            prevLeftBound.AddComponent<LineRenderer>();
            prevLeftBoundRenderer = prevLeftBound.GetComponent<LineRenderer>();
            createLine(prevLeftBoundRenderer, mat, Color.red);

            prevRightBound = new GameObject();
            prevRightBound.AddComponent<LineRenderer>();
            prevRightBoundRenderer = prevRightBound.GetComponent<LineRenderer>();
            createLine(prevRightBoundRenderer, mat, Color.red);
        }

        private void Update() {
            try {
                if (ascendBeam == null) {
                    ascendBeam = GameObject.Find("Ascend Beam");
                }

                if (eyeBeamGlow == null) {
                    eyeBeamGlow = GameObject.Find("Eye Beam Glow");
                }

                if (attackChoices.ActiveStateName == "A2 End" &&
                    gameObject.transform.position.y >= 150f &&
                    eyeBeamGlow != null &&
                    ascendBeam != null &&
                    !ascensionCompleted) { // Ascension has begun and Radiance has started firing beams
                    if (knight.transform.position.y >= 150f) {
                        destroyLines();
                        ascensionCompleted = true;
                    } else {
                        if (ascendBeamControl == null) {
                            ascendBeamControl = ascendBeam.LocateMyFSM("Control");
                        }

                        if (!linesInitialized) {
                            Log("Ascension has begun, adding range indicators");
                            leftBoundRenderer.SetPosition(0, eyeBeamGlow.transform.position + beamWidthOffsetLeft);
                            rightBoundRenderer.SetPosition(0, eyeBeamGlow.transform.position + beamWidthOffsetRight);
                            prevLeftBoundRenderer.SetPosition(0, eyeBeamGlow.transform.position + beamWidthOffsetLeft);
                            prevRightBoundRenderer.SetPosition(0, eyeBeamGlow.transform.position + beamWidthOffsetRight);
                            linesInitialized = true;

                            attackCommands.AddAction("Aim", (FsmStateAction)new CallMethod {
                                behaviour = this,
                                methodName = "UpdatePreviousIndicators",
                                parameters = new FsmVar[0],
                                everyFrame = false,
                            });
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
            Vector3 knightPos = knight.transform.position;
            Vector3 eyeBeamGlowPos = eyeBeamGlow.transform.position;

            leftBoundRenderer.SetPosition(1, Quaternion.Euler(0, 0, 5) * (knightPos - new Vector3(0, -0.5f, 0) - eyeBeamGlowPos) + eyeBeamGlowPos + beamWidthOffsetLeft);
            rightBoundRenderer.SetPosition(1, Quaternion.Euler(0, 0, -5) * (knightPos - new Vector3(0, -0.5f, 0) - eyeBeamGlowPos) + eyeBeamGlowPos + beamWidthOffsetRight);

            // Extend left bound indicator past the player
            Vector3[] leftBoundPositions = new Vector3[2];
            leftBoundRenderer.GetPositions(leftBoundPositions);
            float leftBoundSlope = (leftBoundPositions[0].x - leftBoundPositions[1].x) / (leftBoundPositions[0].y - leftBoundPositions[1].y);
            leftBoundRenderer.SetPosition(1, new Vector3(leftBoundPositions[1].x - 100 * leftBoundSlope, leftBoundPositions[1].y - 100));

            // Extend right bound indicator past the player
            Vector3[] rightBoundPositions = new Vector3[2];
            rightBoundRenderer.GetPositions(rightBoundPositions);
            float rightBoundSlope = (rightBoundPositions[0].x - rightBoundPositions[1].x) / (rightBoundPositions[0].y - rightBoundPositions[1].y);
            rightBoundRenderer.SetPosition(1, new Vector3(rightBoundPositions[1].x - 100 * rightBoundSlope, rightBoundPositions[1].y - 100));
        }

        public void UpdatePreviousIndicators() {
            prevLeftBoundRenderer.SetPosition(1, leftBoundRenderer.GetPosition(1));
            prevRightBoundRenderer.SetPosition(1, rightBoundRenderer.GetPosition(1));
        }

        private void destroyLines() {
            attackCommands.RemoveAction("Aim", 12);
            Destroy(leftBound);
            Destroy(rightBound);
            Destroy(prevLeftBound);
            Destroy(prevRightBound);
            Destroy(this);
        }

        private static void Log(object obj) {
            Logger.Log("[AscendBeamRangeIndicators] - " + obj);
        }
    }
}