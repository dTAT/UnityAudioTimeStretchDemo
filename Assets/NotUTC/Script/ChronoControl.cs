using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 時間系まとめてコントロールする
/// </summary>
public class ChronoControl : MonoBehaviour {
	[SerializeField]
	List<AudioSource> audioSources = new List<AudioSource> ();
	[SerializeField]
	AudioMixerSnapshot snapshotSlow;
	[SerializeField]
	AudioMixerSnapshot snapshotFast;
	[SerializeField]
	AudioMixerSnapshot snapshotDefault;
	[SerializeField]
	AudioMixer defaultMixer;
	/// <summary>
	/// イベントトリガー
	/// </summary>
	class Trigger {
		/// <summary>
		/// 監視するスイッチの前状態
		/// </summary>
		bool lastState = false;
		/// <summary>
		///　スイッチが入った時のイベント 
		/// </summary>
		public UnityEngine.Events.UnityEvent OnTrigger = new UnityEngine.Events.UnityEvent ();
		/// <summary>
		/// スイッチが離された時のイベント
		/// </summary>
		public UnityEngine.Events.UnityEvent OnRelease = new UnityEngine.Events.UnityEvent ();
		/// <summary>
		/// 状態更新
		/// </summary>
		/// <param name="state">スイッチ状態</param>
		public void Update (bool state) {
			if (!lastState && state) {
				OnTrigger.Invoke ();
			}
			if (lastState && !state) {
				OnRelease.Invoke ();
			}
			lastState = state;
		}
	}
	Trigger overClocking = new Trigger ();
	Trigger underClocking = new Trigger ();
	void Start () {
		underClocking.OnTrigger.AddListener (UnderClock);
		underClocking.OnRelease.AddListener (() => RestoreClock (snapshotSlow));
		overClocking.OnTrigger.AddListener (OverClock);
		overClocking.OnRelease.AddListener (() => RestoreClock (snapshotFast));
	}

	void Update () {
		var fast = (Input.GetButton ("Fire1"));
		var slow = (Input.GetButton ("Fire2"));
		overClocking.Update (fast);
		underClocking.Update (!fast && slow);
	}
	const float Duration = 0.05f;
	/// <summary>
	/// 時間進行を進める
	/// </summary>
	void OverClock () {
		var s = 2.0f;
		SetPitch (s);
		SetTimeScale (s);
		SetMixerSnapshot (snapshotFast, Duration);
	}
	/// <summary>
	/// 時間進行を緩める
	/// </summary>
	void UnderClock () {
		var s = 0.5f;
		SetPitch (s);
		SetTimeScale (s);
		SetMixerSnapshot (snapshotSlow, Duration);
	}
	/// <summary>
	/// 時間進行を戻す
	/// </summary>
	/// <param name="snapshot"></param>
	void RestoreClock (AudioMixerSnapshot snapshot) {
		var s = 1.0f;
		SetPitch (s);
		SetTimeScale (s);
		ResetMixerSnapshot (snapshot, Duration);
	}
	/// <summary>
	/// タイムスケール変更
	/// </summary>
	/// <param name="speed"></param>
	void SetTimeScale (float speed) {
		Time.timeScale = speed;
	}
	/// <summary>
	/// AudioSourceのピッチ変更
	/// </summary>
	/// <param name="speed"></param>
	void SetPitch (float speed) {
		for (int i = 0; i < audioSources.Count; ++i) {
			audioSources[i].pitch = speed;
		}
	}
	/// <summary>
	/// スナップショットをデフォルトに戻す
	/// </summary>
	/// <param name="srcSnSnapshot">現スナップショット</param>
	/// <param name="duration">切り替え時間</param>
	public void ResetMixerSnapshot (AudioMixerSnapshot srcSnSnapshot, float duration) {
		TeransitionSnapshot (srcSnSnapshot, snapshotDefault, duration);
	}
	/// <summary>
	/// スナップショットをデフォルトから切り替える
	/// </summary>
	/// <param name="dstMixerSnapshot">切り替え先スナップショット</param>
	/// <param name="duration">切り替え時間</param>
	public void SetMixerSnapshot (AudioMixerSnapshot dstMixerSnapshot, float duration) {
		TeransitionSnapshot (snapshotDefault, dstMixerSnapshot, duration);
	}
	/// <summary>
	/// スナップショットを切り替える
	/// </summary>
	/// <param name="src">切り替え元スナップショット</param>
	/// <param name="dst">切り替え先スナップショット</param>
	/// <param name="duration">切り替え時間</param>
	void TeransitionSnapshot (AudioMixerSnapshot src, AudioMixerSnapshot dst, float duration) {
		AudioMixerSnapshot[] ss = { src, dst };
		float[] w = { 0.0f, 1.0f };
		defaultMixer.TransitionToSnapshots (ss, w, duration);
	}
}